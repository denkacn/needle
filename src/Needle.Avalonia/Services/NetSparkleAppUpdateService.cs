namespace Needle.Avalonia.Services;

using System.Net.Http;
using global::Avalonia;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Threading;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;

internal sealed class NetSparkleAppUpdateService : IAppUpdateService
{
    private readonly IAppInfoService _appInfo;
    private readonly Ed25519Checker _signatureVerifier;
    private readonly NetSparkleUpdateDownloader _downloader;
    private readonly SparkleUpdater _updater;
    private readonly HttpClient _httpClient = new();

    private AppCastItem? _latestUpdate;

    public NetSparkleAppUpdateService(NetSparkleUpdateOptions options, IAppInfoService appInfo)
    {
        _appInfo = appInfo;
        _signatureVerifier = CreateSignatureVerifier(options);
        _downloader = new NetSparkleUpdateDownloader(_signatureVerifier, _httpClient);
        _updater = new SparkleUpdater(options.AppCastUrl, _signatureVerifier)
        {
            UserInteractionMode = UserInteractionMode.DownloadNoInstall,
            RelaunchAfterUpdate = false,
            UseNotificationToast = false,
            CheckServerFileName = false
        };
    }

    public async Task<AppUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var updateInfo = await _updater.CheckForUpdatesQuietly(ignoreSkippedVersions: true);
            cancellationToken.ThrowIfCancellationRequested();

            if (updateInfo.Status != UpdateStatus.UpdateAvailable || updateInfo.Updates.Count == 0)
            {
                _latestUpdate = null;
                return new AppUpdateCheckResult(false, null, AppCastUpdateInfo.ResolveNoUpdateMessage(updateInfo.Status));
            }

            _latestUpdate = updateInfo.Updates[0];
            var latestVersion = AppCastUpdateInfo.ResolveVersion(_latestUpdate);
            if (!AppCastUpdateInfo.IsNewerVersion(latestVersion, _appInfo.Version))
            {
                _latestUpdate = null;
                return new AppUpdateCheckResult(false, null, "Needle is up to date.");
            }

            return new AppUpdateCheckResult(true, latestVersion);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _latestUpdate = null;
            return new AppUpdateCheckResult(
                false,
                null,
                $"Update check failed: {ex.Message}");
        }
    }

    public async Task DownloadAndApplyUpdateAsync(IProgress<int> progress, CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Single-file self updates are currently configured for Windows builds.");
        }

        var update = _latestUpdate;
        if (update is null)
        {
            var check = await CheckForUpdatesAsync(cancellationToken);
            if (!check.IsAvailable || _latestUpdate is null)
            {
                throw new InvalidOperationException(check.Message ?? "No update is available.");
            }

            update = _latestUpdate;
        }

        var downloadPath = await _downloader.DownloadAsync(update, progress, cancellationToken);
        WindowsSingleFileUpdateLauncher.Start(downloadPath);
        await Task.Delay(500, cancellationToken);
        await ShutdownApplicationAsync();
    }

    private static Ed25519Checker CreateSignatureVerifier(NetSparkleUpdateOptions options)
    {
        var publicKey = options.Ed25519PublicKey ?? string.Empty;
        var mode = string.IsNullOrWhiteSpace(publicKey)
            ? SecurityMode.Unsafe
            : SecurityMode.Strict;

        return new Ed25519Checker(mode, publicKey, string.Empty, readFileBeingVerifiedInChunks: true, chunkSize: 8192);
    }

    private static Task ShutdownApplicationAsync()
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
                return;
            }

            Environment.Exit(0);
        }).GetTask();
    }
}
