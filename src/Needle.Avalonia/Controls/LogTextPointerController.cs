using System.Collections;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Controls;

internal sealed class LogTextPointerController
{
    private readonly LogTextHitTester _hitTester = new();
    private bool _isSelectingText;

    public LogTextSelection Selection { get; } = new();

    public bool SelectAt(LogTextPointerContext context, Point position, bool beginTextSelection, IPointer? pointer = null)
    {
        var line = _hitTester.GetLineAt(context.Lines, position.Y, context.LineHeight);
        if (line is null)
        {
            return false;
        }

        context.SetSelectedLineNumber(line.LineNumber);
        Selection.Start(line, GetCharacterIndex(context, line.Text, position.X));
        context.SetSelectedText(string.Empty);

        if (beginTextSelection)
        {
            _isSelectingText = position.X >= context.GutterWidth;
            pointer?.Capture(context.CaptureTarget);
            context.Invalidate();
            return true;
        }

        context.Invalidate();
        return true;
    }

    public bool Move(LogTextPointerContext context, Point position)
    {
        if (!_isSelectingText || Selection.Line is null)
        {
            return false;
        }

        var line = _hitTester.GetLineAt(context.Lines, position.Y, context.LineHeight);
        if (line?.LineNumber != Selection.Line.LineNumber)
        {
            return false;
        }

        Selection.MoveTo(GetCharacterIndex(context, Selection.Line.Text, position.X));
        context.SetSelectedText(Selection.Text);
        context.Invalidate();
        return true;
    }

    public bool Release(LogTextPointerContext context, Point position, IPointer pointer)
    {
        if (!_isSelectingText)
        {
            return false;
        }

        _isSelectingText = false;
        pointer.Capture(null);
        if (Selection.Line is not null)
        {
            Selection.MoveTo(GetCharacterIndex(context, Selection.Line.Text, position.X));
            context.SetSelectedText(Selection.Text);
        }

        context.Invalidate();
        return true;
    }

    private int GetCharacterIndex(LogTextPointerContext context, string text, double x)
    {
        return _hitTester.GetCharacterIndex(
            text,
            x,
            context.GutterWidth,
            context.LineHeight,
            context.TextFontSize,
            context.TextFontFamily);
    }
}

internal sealed record LogTextPointerContext(
    IEnumerable? Lines,
    double LineHeight,
    double GutterWidth,
    double TextFontSize,
    FontFamily TextFontFamily,
    IInputElement CaptureTarget,
    Action<long> SetSelectedLineNumber,
    Action<string> SetSelectedText,
    Action Invalidate);
