namespace Needle.Application.Preferences;

using Needle.Application.Workspace;

public sealed class UserPreferences
{
    public int SettingsVersion { get; init; } = 1;

    public string LogFontFamilyName { get; init; } = "Cascadia Mono";

    public List<string> CustomLogFontFamilyNames { get; init; } = [];

    public double LogFontSize { get; init; } = 13;

    public string LogTextColor { get; init; } = string.Empty;

    public string TriggerHighlightColor { get; init; } = string.Empty;

    public bool IsAlwaysOnTop { get; init; }

    public bool IsAutoFollowTailEnabled { get; init; } = true;

    public bool IsMiniMapEnabled { get; init; } = true;

    public bool IsStructuredMode { get; init; }

    public List<string> RecentFiles { get; init; } = [];

    public bool HighlightRulesConfigured { get; init; }

    public List<WorkspaceHighlightRule> HighlightRules { get; init; } = [];

    public WindowPreference MainWindow { get; init; } = new();

    public WindowPreference SettingsWindow { get; init; } = new();
}

public sealed class WindowPreference
{
    public double? X { get; init; }

    public double? Y { get; init; }

    public double Width { get; init; }

    public double Height { get; init; }
}
