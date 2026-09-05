namespace Needle.Avalonia.Services;

internal sealed class NoOpAppUpdateService : IAppUpdateService
{
    public Task<AppUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new AppUpdateCheckResult(
            IsAvailable: false,
            Version: null,
            Message: "Updates are not configured for this build."));
    }

    public Task DownloadAndApplyUpdateAsync(IProgress<int> progress, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
