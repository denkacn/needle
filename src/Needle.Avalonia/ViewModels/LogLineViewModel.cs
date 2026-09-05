namespace Needle.Avalonia.ViewModels;

public sealed record LogLineViewModel(
    long LineNumber,
    string Text,
    string? Foreground = null,
    string? Background = null,
    string? Accent = null,
    bool IsBookmarked = false,
    bool IsSelected = false,
    bool IsTriggered = false);
