namespace Needle.Avalonia.Controls;

using System.Collections;
using global::Avalonia.Media;
using global::Avalonia.Media.TextFormatting;
using Needle.Avalonia.ViewModels;

internal sealed class LogTextHitTester
{
    public LogLineViewModel? GetLineAt(IEnumerable? lines, double y, double lineHeight)
    {
        var index = (int)Math.Floor(y / lineHeight);
        if (index < 0 || lines is null)
        {
            return null;
        }

        var currentIndex = 0;
        foreach (var item in lines)
        {
            if (item is LogLineViewModel line)
            {
                if (currentIndex == index)
                {
                    return line;
                }

                currentIndex++;
            }
        }

        return null;
    }

    public int GetCharacterIndex(
        string text,
        double x,
        double gutterWidth,
        double lineHeight,
        double textFontSize,
        FontFamily textFontFamily)
    {
        if (string.IsNullOrEmpty(text) || x <= gutterWidth + 12)
        {
            return 0;
        }

        var textX = x - gutterWidth - 12;
        var layout = new TextLayout(
            text,
            new Typeface(textFontFamily),
            textFontSize,
            Brushes.Black,
            textWrapping: TextWrapping.NoWrap,
            maxWidth: 1200,
            maxHeight: lineHeight);

        if (layout.Width <= 0)
        {
            return 0;
        }

        var characterWidth = layout.Width / text.Length;
        return Math.Clamp((int)Math.Round(textX / characterWidth), 0, text.Length);
    }
}
