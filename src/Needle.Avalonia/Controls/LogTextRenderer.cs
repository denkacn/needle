namespace Needle.Avalonia.Controls;

using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using global::Avalonia;
using global::Avalonia.Media;
using global::Avalonia.Media.TextFormatting;
using Needle.Avalonia.ViewModels;

internal sealed class LogTextRenderer
{
    private static readonly ConcurrentDictionary<string, IBrush> BrushCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Geometry BookmarkIconGeometry =
        Geometry.Parse("M2,1 L12,1 L12,15 L7,12 L2,15 Z");

    public void Render(DrawingContext context, IEnumerable? lines, LogTextSelection selection, LogTextRenderSettings settings)
    {
        context.DrawRectangle(settings.GutterBrush, null, new Rect(0, 0, settings.GutterWidth, settings.AvailableHeight));

        if (lines is null)
        {
            return;
        }

        var typeface = new Typeface(settings.TextFontFamily);
        var y = 0d;
        foreach (var item in lines)
        {
            if (item is not LogLineViewModel line)
            {
                continue;
            }

            if (y > settings.AvailableHeight)
            {
                break;
            }

            DrawLineBackground(context, line, settings, y);
            DrawSelectedLineBookmarkToggle(context, line, settings, y);
            DrawLineNumber(context, line, typeface, settings, y);
            DrawLineText(context, line, selection, typeface, settings, y);
            y += settings.LineHeight;
        }
    }

    private static void DrawSelectedLineBookmarkToggle(
        DrawingContext context,
        LogLineViewModel line,
        LogTextRenderSettings settings,
        double y)
    {
        if (line.LineNumber != settings.SelectedLineNumber)
        {
            return;
        }

        var iconY = y + Math.Max(3, (settings.LineHeight - 16) / 2);
        using var transform = context.PushTransform(Matrix.CreateTranslation(7, iconY));
        if (line.IsBookmarked)
        {
            context.DrawGeometry(ResolveBrush("#E0A800", settings.LineNumberBrush), null, BookmarkIconGeometry);
            return;
        }

        context.DrawGeometry(null, new Pen(settings.LineNumberBrush, 1.4), BookmarkIconGeometry);
    }

    private static void DrawLineNumber(
        DrawingContext context,
        LogLineViewModel line,
        Typeface typeface,
        LogTextRenderSettings settings,
        double y)
    {
        var layout = new TextLayout(
            line.LineNumber.ToString(CultureInfo.CurrentCulture),
            typeface,
            settings.TextFontSize,
            settings.LineNumberBrush,
            textAlignment: TextAlignment.Right,
            maxWidth: ToFiniteWidth(Math.Max(0, settings.GutterWidth - 12)),
            maxHeight: settings.LineHeight);

        layout.Draw(context, new Point(0, y + 2));
    }

    private static void DrawLineText(
        DrawingContext context,
        LogLineViewModel line,
        LogTextSelection selection,
        Typeface typeface,
        LogTextRenderSettings settings,
        double y)
    {
        var brush = ResolveBrush(line.Foreground, settings.TextBrush) ?? settings.TextBrush;
        var layout = new TextLayout(
            line.Text,
            typeface,
            settings.TextFontSize,
            brush,
            textWrapping: TextWrapping.NoWrap,
            maxWidth: double.PositiveInfinity,
            maxHeight: settings.LineHeight);

        DrawTextSelection(context, line, selection, layout, settings, y);
        layout.Draw(context, new Point(settings.GutterWidth + 12, y + 2));
    }

    private static void DrawTextSelection(
        DrawingContext context,
        LogLineViewModel line,
        LogTextSelection selection,
        TextLayout layout,
        LogTextRenderSettings settings,
        double y)
    {
        if (selection.Line?.LineNumber != line.LineNumber || selection.IsEmpty)
        {
            return;
        }

        var startX = selection.GetStartX(layout);
        var endX = selection.GetEndX(layout);
        var width = Math.Max(1, endX - startX);
        context.DrawRectangle(settings.TextSelectionBrush, null, new Rect(settings.GutterWidth + 12 + startX, y + 2, width, Math.Max(1, settings.LineHeight - 4)));
    }

    private static void DrawLineBackground(
        DrawingContext context,
        LogLineViewModel line,
        LogTextRenderSettings settings,
        double y)
    {
        context.DrawRectangle(Brushes.Transparent, null, new Rect(0, y, settings.AvailableWidth, settings.LineHeight));

        if (line.IsSelected || line.LineNumber == settings.SelectedLineNumber)
        {
            context.DrawRectangle(settings.SelectedLineBrush, null, new Rect(0, y, settings.AvailableWidth, settings.LineHeight));
        }

        var background = line.IsBookmarked && IsDefaultBookmarkBackground(line.Background)
            ? settings.BookmarkBackgroundBrush
            : ResolveBrush(line.Background, null);
        if (background is null && line.IsTriggered)
        {
            background = settings.TriggerBackgroundBrush;
        }

        if (background is not null && !line.IsSelected && line.LineNumber != settings.SelectedLineNumber)
        {
            context.DrawRectangle(background, null, new Rect(0, y, settings.AvailableWidth, settings.LineHeight));
        }

        var accent = ResolveBrush(line.Accent, null);
        if (accent is not null)
        {
            context.DrawRectangle(accent, null, new Rect(0, y, 3, settings.LineHeight));
        }

    }

    private static double ToFiniteWidth(double width)
    {
        return double.IsFinite(width) && width > 0 ? width : 1200;
    }

    private static IBrush? ResolveBrush(string? value, IBrush? fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return BrushCache.GetOrAdd(value, static color => new SolidColorBrush(Color.Parse(color)));
    }

    private static bool IsDefaultBookmarkBackground(string? value)
    {
        return string.Equals(value, "#2B2412", StringComparison.OrdinalIgnoreCase);
    }
}
