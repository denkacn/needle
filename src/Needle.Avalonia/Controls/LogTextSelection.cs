namespace Needle.Avalonia.Controls;

using global::Avalonia;
using global::Avalonia.Media.TextFormatting;
using Needle.Avalonia.ViewModels;

internal sealed class LogTextSelection
{
    public LogLineViewModel? Line { get; private set; }

    public int Anchor { get; private set; }

    public int Current { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public bool IsEmpty => Line is null || Anchor == Current;

    public void Start(LogLineViewModel line, int characterIndex)
    {
        Line = line;
        Anchor = characterIndex;
        Current = characterIndex;
        Text = string.Empty;
    }

    public void MoveTo(int characterIndex)
    {
        if (Line is null)
        {
            Text = string.Empty;
            return;
        }

        Current = characterIndex;
        Text = GetSelectedText(Line.Text);
    }

    public double GetStartX(TextLayout layout)
    {
        return GetCharacterX(layout, Math.Min(Anchor, Current));
    }

    public double GetEndX(TextLayout layout)
    {
        return GetCharacterX(layout, Math.Max(Anchor, Current));
    }

    private string GetSelectedText(string lineText)
    {
        if (Anchor == Current)
        {
            return string.Empty;
        }

        var start = Math.Min(Anchor, Current);
        var end = Math.Max(Anchor, Current);
        return lineText.Substring(start, end - start);
    }

    private double GetCharacterX(TextLayout layout, int index)
    {
        if (Line is null || string.IsNullOrEmpty(Line.Text) || index <= 0 || layout.Width <= 0)
        {
            return 0;
        }

        if (index >= Line.Text.Length)
        {
            return layout.Width;
        }

        return layout.Width * index / Line.Text.Length;
    }
}
