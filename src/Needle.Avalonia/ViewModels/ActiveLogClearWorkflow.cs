namespace Needle.Avalonia.ViewModels;

using Needle.Application.Documents;

internal sealed class ActiveLogClearWorkflow
{
    public async Task ClearAsync(ActiveLogClearContext context)
    {
        var path = context.ActivePath;
        if (!context.Document.IsDocumentOpen || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            context.Document.Status = LogStatusFormatter.ActiveFileNotFound;
            context.NotifyDocumentStateChanged();
            return;
        }

        try
        {
            await TruncateFileAsync(path);
            await context.RefreshSessionAfterTruncateAsync();
            context.SearchFilter.SearchMatches.Clear();
            context.SearchFilter.FilteredLineNumbers.Clear();
            context.RefreshMaximumScrollLine();
            context.Document.ScrollLine = 0;
            await context.ReloadViewportAsync();
            context.RefreshBookmarkCommands();
            context.Document.Status = "Log file cleared";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            context.Document.Status = $"Failed to clear log file: {ex.Message}";
        }

        context.NotifyDocumentStateChanged();
    }

    private static async Task TruncateFileAsync(string path)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Write,
            FileShare.ReadWrite | FileShare.Delete);
        stream.SetLength(0);
    }
}

internal sealed record ActiveLogClearContext(
    string? ActivePath,
    DocumentStateViewModel Document,
    SearchFilterViewModel SearchFilter,
    Func<Task> RefreshSessionAfterTruncateAsync,
    Action RefreshMaximumScrollLine,
    Func<Task> ReloadViewportAsync,
    Action RefreshBookmarkCommands,
    Action NotifyDocumentStateChanged);
