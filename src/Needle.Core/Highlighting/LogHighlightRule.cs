namespace Needle.Core.Highlighting;

public sealed record LogHighlightRule(
    string Pattern,
    bool IsRegex,
    string? Foreground,
    string? Background,
    bool Enabled = true);
