namespace Needle.Application.Workspace;

using System.Text.Json.Serialization;

public sealed class WorkspaceState
{
    public string? ActiveDocumentPath { get; init; }

    public bool HighlightRulesConfigured { get; init; }

    public List<WorkspaceHighlightRule> HighlightRules { get; init; } = [];

    public List<WorkspaceDocumentState> Documents { get; init; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WorkspaceWindowState? MainWindow { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WorkspaceWindowState? SettingsWindow { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WorkspaceUiSettings? UiSettings { get; init; }
}

public sealed class WorkspaceUiSettings
{
    public string LogTextColor { get; init; } = string.Empty;
}

public sealed class WorkspaceWindowState
{
    public double Width { get; init; }

    public double Height { get; init; }
}

public sealed class WorkspaceHighlightRule
{
    public string Pattern { get; init; } = string.Empty;

    public bool IsRegex { get; init; }

    public string? Foreground { get; init; }

    public string? Background { get; init; }

    public bool Enabled { get; init; } = true;
}

public sealed class WorkspaceDocumentState
{
    public string Path { get; init; } = string.Empty;

    public long FirstVisibleLine { get; init; }

    public string SearchText { get; init; } = string.Empty;

    public string FilterText { get; init; } = string.Empty;

    public List<string> ExcludePatterns { get; init; } = [];

    public List<string> TriggerPatterns { get; init; } = [];

    public bool HighlightRulesConfigured { get; init; }

    public List<WorkspaceHighlightRule> HighlightRules { get; init; } = [];

    public bool IsFilterActive { get; init; }

    public bool IsTailPaused { get; init; }

    public bool IsFollowingTail { get; init; } = true;

    public bool IsStructuredMode { get; init; }

    public List<long> Bookmarks { get; init; } = [];
}
