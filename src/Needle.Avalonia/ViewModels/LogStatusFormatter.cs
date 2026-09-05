namespace Needle.Avalonia.ViewModels;

internal static class LogStatusFormatter
{
    public const string NoLogFileOpened = "No log file opened";
    public const string OpenCancelled = "Open cancelled";
    public const string ActiveFileNotFound = "Active file not found";
    public const string Searching = "Searching...";
    public const string SearchCancelled = "Search cancelled";
    public const string Filtering = "Filtering...";
    public const string FilterCancelled = "Filter cancelled";
    public const string NoFilteredLines = "No filtered lines";
    public const string PressApply = "Press Apply";
    public const string DropALogFile = "Drop a log file";

    public static string Opening(string displayName)
    {
        return $"Opening {displayName}...";
    }

    public static string Indexing(LogTabOpenProgress progress)
    {
        return $"Indexing {progress.DisplayName} ({FormatBytes(progress.FileSize)}, {progress.EncodingDisplayName})...";
    }

    public static string OpenFailed(Exception exception)
    {
        return $"Could not open file: {exception.Message}";
    }

    public static string Viewport(
        long lineCount,
        long fileSize,
        string viewportRange,
        bool isFilterActive,
        int filteredLineCount)
    {
        var filterSuffix = isFilterActive ? $"   Visible: {filteredLineCount:N0}" : string.Empty;
        return $"Lines: {lineCount:N0}   Size: {FormatBytes(fileSize)}   View: {viewportRange}{filterSuffix}";
    }

    public static string TailReset(LogTailUpdate update, long lineCount)
    {
        return update.ResetReason == "rotation"
            ? $"Log rotation detected. Reindexed current file: {lineCount:N0} lines"
            : $"Log truncation detected. Reindexed current file: {lineCount:N0} lines";
    }

    public static string TailAdded(LogTailUpdate update, long lineCount)
    {
        return $"+{update.AddedLineCount:N0} new lines   Lines: {lineCount:N0}";
    }

    public static string ExternalActionFailed(string failurePrefix, Exception exception)
    {
        return $"{failurePrefix}: {exception.Message}";
    }

    public static string SearchFailed(Exception exception)
    {
        return $"Search failed: {exception.Message}";
    }

    public static string FirstFilteredLines(int limit)
    {
        return $"First {limit:N0} lines";
    }

    public static string FilteredLines(int count)
    {
        return $"{count:N0} filtered lines";
    }

    public static string FilterFailed(Exception exception)
    {
        return $"Filter failed: {exception.Message}";
    }

    public static string CopiedLine(long lineNumber)
    {
        return $"Copied line {lineNumber:N0}";
    }

    public static string CopiedSelection(long lineNumber)
    {
        return $"Copied selection from line {lineNumber:N0}";
    }

    public static string SelectedLine(long lineNumber)
    {
        return $"Line {lineNumber:N0} selected";
    }

    public static string RecentFileNotFound(string path)
    {
        return $"Recent file not found: {path}";
    }

    public static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var value = (double)bytes;
        var unit = 0;

        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} B" : $"{value:0.##} {units[unit]}";
    }
}
