namespace Needle.Avalonia.ViewModels;

using Needle.Application.Preferences;
using Needle.Application.Workspace;

internal static class UserPreferencesMapper
{
    public static UserPreferences Create(UserPreferencesSnapshot snapshot)
    {
        return new UserPreferences
        {
            LogFontFamilyName = snapshot.DisplaySettings.LogFontFamilyName,
            CustomLogFontFamilyNames = [.. snapshot.DisplaySettings.CustomLogFontFamilyNames],
            LogFontSize = snapshot.DisplaySettings.LogFontSize,
            LogTextColor = snapshot.DisplaySettings.LogTextColor,
            TriggerHighlightColor = snapshot.DisplaySettings.TriggerHighlightColor,
            IsAlwaysOnTop = snapshot.DisplaySettings.IsAlwaysOnTop,
            IsAutoFollowTailEnabled = snapshot.DisplaySettings.IsAutoFollowTailEnabled,
            IsMiniMapEnabled = snapshot.DisplaySettings.IsMiniMapEnabled,
            IsStructuredMode = snapshot.IsStructuredMode,
            RecentFiles = [.. snapshot.RecentFilePaths],
            HighlightRulesConfigured = true,
            HighlightRules = [.. snapshot.HighlightRules],
            MainWindow = new WindowPreference
            {
                X = snapshot.DisplaySettings.HasMainWindowPosition
                    ? snapshot.DisplaySettings.MainWindowX
                    : null,
                Y = snapshot.DisplaySettings.HasMainWindowPosition
                    ? snapshot.DisplaySettings.MainWindowY
                    : null,
                Width = snapshot.DisplaySettings.MainWindowWidth,
                Height = snapshot.DisplaySettings.MainWindowHeight
            },
            SettingsWindow = new WindowPreference
            {
                Width = snapshot.DisplaySettings.SettingsWindowWidth,
                Height = snapshot.DisplaySettings.SettingsWindowHeight
            }
        };
    }
}

internal sealed record UserPreferencesSnapshot(
    DisplaySettingsViewModel DisplaySettings,
    bool IsStructuredMode,
    IReadOnlyList<string> RecentFilePaths,
    IReadOnlyList<WorkspaceHighlightRule> HighlightRules);
