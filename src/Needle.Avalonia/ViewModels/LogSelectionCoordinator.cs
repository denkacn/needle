namespace Needle.Avalonia.ViewModels;

internal sealed class LogSelectionCoordinator
{
    public LogSelectionState Clear()
    {
        return new LogSelectionState(
            SelectedLineText: string.Empty,
            SelectedText: string.Empty);
    }

    public LogSelectionState Refresh(
        long selectedLineNumber,
        IEnumerable<LogLineViewModel> visibleLines,
        string currentSelectedLineText)
    {
        if (selectedLineNumber <= 0)
        {
            return Clear();
        }

        var selectedLineText = visibleLines.FirstOrDefault(line => line.LineNumber == selectedLineNumber)?.Text
            ?? currentSelectedLineText;
        return new LogSelectionState(
            selectedLineText,
            SelectedText: string.Empty);
    }

    public string? GetTextToCopy(string selectedText, string selectedLineText)
    {
        var text = !string.IsNullOrEmpty(selectedText) ? selectedText : selectedLineText;
        return string.IsNullOrEmpty(text) ? null : text;
    }
}

internal sealed record LogSelectionState(
    string SelectedLineText,
    string SelectedText);
