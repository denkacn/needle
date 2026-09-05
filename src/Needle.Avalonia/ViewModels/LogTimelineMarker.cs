namespace Needle.Avalonia.ViewModels;

public sealed record LogTimelineMarker(long LineNumber, LogTimelineMarkerKind Kind, string? Color = null);

public enum LogTimelineMarkerKind
{
    Error,
    Warning,
    Highlight,
    Search,
    Bookmark,
    Trigger
}
