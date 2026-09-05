namespace Needle.Avalonia.Controls;

using System.Collections;
using global::Avalonia;
using global::Avalonia.Media;
using global::Avalonia.Media.TextFormatting;
using Needle.Avalonia.ViewModels;

internal static class LogTextMetrics
{
    private const double DefaultDesiredWidth = 1200;

    public static Size EstimateDesiredSize(
        IEnumerable? lines,
        Size availableSize,
        double lineHeight,
        double gutterWidth,
        double textFontSize,
        FontFamily textFontFamily)
    {
        var lineCount = 0;
        var maxTextWidth = 0d;
        var typeface = new Typeface(textFontFamily);

        if (lines is not null)
        {
            foreach (var item in lines)
            {
                if (item is not LogLineViewModel line)
                {
                    continue;
                }

                lineCount++;
                maxTextWidth = Math.Max(maxTextWidth, MeasureTextWidth(line.Text, typeface, textFontSize, lineHeight));
            }
        }

        var desiredWidth = gutterWidth + 44 + maxTextWidth;
        var viewportWidth = ToFiniteWidth(availableSize.Width);
        return new Size(Math.Max(viewportWidth, desiredWidth), Math.Max(0, lineCount * lineHeight));
    }

    public static double ToFiniteWidth(double width)
    {
        return double.IsFinite(width) && width > 0 ? width : DefaultDesiredWidth;
    }

    private static double MeasureTextWidth(string text, Typeface typeface, double textFontSize, double lineHeight)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        try
        {
            var layout = new TextLayout(
                text,
                typeface,
                textFontSize,
                Brushes.Transparent,
                textWrapping: TextWrapping.NoWrap,
                maxWidth: double.PositiveInfinity,
                maxHeight: lineHeight);

            return layout.Width;
        }
        catch (InvalidOperationException)
        {
            return text.Length * textFontSize * 0.62;
        }
    }
}
