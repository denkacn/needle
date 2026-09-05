using Avalonia.Input.Platform;

namespace Needle.Avalonia.Controls;

internal static class LogTextClipboardController
{
    public static async Task CopySelectionAsync(LogTextSelection selection, string selectedText, IClipboard? clipboard)
    {
        if (clipboard is null)
        {
            return;
        }

        var text = !string.IsNullOrEmpty(selectedText)
            ? selectedText
            : selection.Line?.Text;

        if (!string.IsNullOrEmpty(text))
        {
            await clipboard.SetTextAsync(text);
        }
    }
}
