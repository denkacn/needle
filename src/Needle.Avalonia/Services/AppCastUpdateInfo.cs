using NetSparkleUpdater;
using NetSparkleUpdater.Enums;

namespace Needle.Avalonia.Services;

internal static class AppCastUpdateInfo
{
    public static string ResolveVersion(AppCastItem item)
    {
        var version = !string.IsNullOrWhiteSpace(item.ShortVersion)
            ? item.ShortVersion
            : item.Version;

        return string.IsNullOrWhiteSpace(version)
            ? "unknown"
            : version.TrimStart('v', 'V');
    }

    public static bool IsNewerVersion(string candidate, string current)
    {
        if (Version.TryParse(candidate, out var candidateVersion)
            && Version.TryParse(current, out var currentVersion))
        {
            return candidateVersion > currentVersion;
        }

        return !string.Equals(candidate, current, StringComparison.OrdinalIgnoreCase);
    }

    public static string ResolveNoUpdateMessage(UpdateStatus status)
    {
        return status switch
        {
            UpdateStatus.UpdateNotAvailable => "Needle is up to date.",
            UpdateStatus.UserSkipped => "This update was skipped.",
            _ => "Could not determine whether an update is available."
        };
    }
}
