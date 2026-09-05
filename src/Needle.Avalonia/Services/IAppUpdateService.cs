namespace Needle.Avalonia.Services;

public interface IAppUpdateService
{
    Task<AppUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken);

    Task DownloadAndApplyUpdateAsync(IProgress<int> progress, CancellationToken cancellationToken);
}

public sealed record AppUpdateCheckResult(
    bool IsAvailable,
    string? Version,
    string? Message = null);
