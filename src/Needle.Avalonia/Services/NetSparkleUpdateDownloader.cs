using System.Net.Http;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;

namespace Needle.Avalonia.Services;

internal sealed class NetSparkleUpdateDownloader
{
    private readonly Ed25519Checker _signatureVerifier;
    private readonly HttpClient _httpClient;

    public NetSparkleUpdateDownloader(Ed25519Checker signatureVerifier, HttpClient httpClient)
    {
        _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string> DownloadAsync(
        AppCastItem update,
        IProgress<int> progress,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(update.DownloadLink))
        {
            throw new InvalidOperationException("The update appcast item does not contain a download link.");
        }

        var version = AppCastUpdateInfo.ResolveVersion(update);
        var downloadUri = new Uri(update.DownloadLink, UriKind.Absolute);
        var fileName = Path.GetFileName(downloadUri.LocalPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"Needle-{version}-win-x64.exe";
        }

        var updateDirectory = Path.Combine(Path.GetTempPath(), "Needle", "updates", version);
        Directory.CreateDirectory(updateDirectory);

        var downloadPath = Path.Combine(updateDirectory, fileName);
        using var response = await _httpClient.GetAsync(downloadUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await WriteContentAsync(response, downloadPath, progress, cancellationToken);
        progress.Report(100);
        VerifyDownloadedUpdate(update, downloadPath);
        return downloadPath;
    }

    private static async Task WriteContentAsync(
        HttpResponseMessage response,
        string downloadPath,
        IProgress<int> progress,
        CancellationToken cancellationToken)
    {
        var totalBytes = response.Content.Headers.ContentLength;
        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var target = File.Create(downloadPath);

        var buffer = new byte[128 * 1024];
        long receivedBytes = 0;
        int read;
        while ((read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            receivedBytes += read;

            if (totalBytes is > 0)
            {
                progress.Report((int)Math.Clamp(receivedBytes * 100 / totalBytes.Value, 0, 100));
            }
        }
    }

    private void VerifyDownloadedUpdate(AppCastItem update, string downloadPath)
    {
        var result = _signatureVerifier.VerifySignatureOfFile(update.DownloadSignature ?? string.Empty, downloadPath);
        if (result == ValidationResult.Valid || result == ValidationResult.Unchecked)
        {
            return;
        }

        throw new InvalidOperationException("Downloaded update signature is invalid.");
    }
}
