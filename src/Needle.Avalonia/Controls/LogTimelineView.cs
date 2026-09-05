namespace Needle.Avalonia.Controls;

using System.Collections;
using System.Collections.Specialized;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Media;
using Needle.Avalonia.ViewModels;

public sealed class LogTimelineView : Control
{
    public static readonly StyledProperty<IEnumerable?> MarkersProperty =
        AvaloniaProperty.Register<LogTimelineView, IEnumerable?>(nameof(Markers));

    public static readonly StyledProperty<long> TotalLineCountProperty =
        AvaloniaProperty.Register<LogTimelineView, long>(nameof(TotalLineCount));

    public static readonly StyledProperty<double> ScrollLineProperty =
        AvaloniaProperty.Register<LogTimelineView, double>(nameof(ScrollLine));

    public static readonly StyledProperty<int> ViewportLineCountProperty =
        AvaloniaProperty.Register<LogTimelineView, int>(nameof(ViewportLineCount));

    public event EventHandler<LogTimelineJumpRequestedEventArgs>? JumpRequested;

    static LogTimelineView()
    {
        AffectsRender<LogTimelineView>(
            MarkersProperty,
            TotalLineCountProperty,
            ScrollLineProperty,
            ViewportLineCountProperty);
    }

    public IEnumerable? Markers
    {
        get => GetValue(MarkersProperty);
        set => SetValue(MarkersProperty, value);
    }

    public long TotalLineCount
    {
        get => GetValue(TotalLineCountProperty);
        set => SetValue(TotalLineCountProperty, value);
    }

    public double ScrollLine
    {
        get => GetValue(ScrollLineProperty);
        set => SetValue(ScrollLineProperty, value);
    }

    public int ViewportLineCount
    {
        get => GetValue(ViewportLineCountProperty);
        set => SetValue(ViewportLineCountProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != MarkersProperty)
        {
            return;
        }

        if (change.OldValue is INotifyCollectionChanged oldMarkers)
        {
            oldMarkers.CollectionChanged -= OnMarkersChanged;
        }

        if (change.NewValue is INotifyCollectionChanged newMarkers)
        {
            newMarkers.CollectionChanged += OnMarkersChanged;
        }

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = new Rect(Bounds.Size);
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#1A1C1F")), null, bounds);
        context.DrawLine(new Pen(new SolidColorBrush(Color.Parse("#373A40")), 1), new Point(0.5, 0), new Point(0.5, Bounds.Height));

        if (TotalLineCount <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        DrawMarkers(context);
        DrawViewport(context);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var point = e.GetPosition(this);
        if (TotalLineCount <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var ratio = Math.Clamp(point.Y / Bounds.Height, 0, 1);
        var lineNumber = (long)Math.Round(ratio * Math.Max(0, TotalLineCount - 1));
        JumpRequested?.Invoke(this, new LogTimelineJumpRequestedEventArgs(lineNumber));
        e.Handled = true;
    }

    private void DrawMarkers(DrawingContext context)
    {
        if (Markers is null)
        {
            return;
        }

        foreach (var item in Markers)
        {
            if (item is not LogTimelineMarker marker)
            {
                continue;
            }

            var y = ToY(marker.LineNumber);
            var brush = BrushFor(marker);
            var height = marker.Kind is LogTimelineMarkerKind.Bookmark or LogTimelineMarkerKind.Trigger ? 3 : 2;
            context.DrawRectangle(brush, null, new Rect(3, y, Math.Max(4, Bounds.Width - 6), height));
        }
    }

    private void DrawViewport(DrawingContext context)
    {
        var visibleRatio = TotalLineCount <= 0 ? 1 : Math.Clamp((double)ViewportLineCount / TotalLineCount, 0.02, 1);
        var height = Math.Max(18, Bounds.Height * visibleRatio);
        var top = GetViewportTop(height);
        var brush = new SolidColorBrush(Color.Parse("#FFFFFF")) { Opacity = 0.16 };
        var pen = new Pen(new SolidColorBrush(Color.Parse("#8B949E")), 1);
        context.DrawRectangle(brush, pen, new Rect(1.5, top, Math.Max(1, Bounds.Width - 3), height), 3);
    }

    private void OnMarkersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private double GetViewportTop(double viewportHeight)
    {
        var firstLine = ToLineNumber(ScrollLine);
        var lastLine = Math.Min(Math.Max(firstLine, firstLine + Math.Max(1, ViewportLineCount) - 1), Math.Max(0, TotalLineCount - 1));
        var centerLine = firstLine + (lastLine - firstLine) / 2d;
        var centerY = ToY(centerLine);
        var top = centerY - viewportHeight / 2d;
        return Math.Clamp(top, 0, Math.Max(0, Bounds.Height - viewportHeight));
    }

    private double ToY(double lineNumber)
    {
        var denominator = Math.Max(1, TotalLineCount - 1);
        return Math.Clamp(lineNumber / denominator * Bounds.Height, 0, Math.Max(0, Bounds.Height - 2));
    }

    private double ToY(long lineNumber)
    {
        return ToY((double)lineNumber);
    }

    private static long ToLineNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 0;
        }

        return Math.Max(0, (long)Math.Round(value));
    }

    private static IBrush BrushFor(LogTimelineMarker marker)
    {
        if (marker.Kind == LogTimelineMarkerKind.Highlight &&
            !string.IsNullOrWhiteSpace(marker.Color) &&
            Color.TryParse(marker.Color, out var color))
        {
            return new SolidColorBrush(color);
        }

        return marker.Kind switch
        {
            LogTimelineMarkerKind.Error => Brushes.Red,
            LogTimelineMarkerKind.Warning => new SolidColorBrush(Color.Parse("#D7A63F")),
            LogTimelineMarkerKind.Highlight => new SolidColorBrush(Color.Parse("#8B5CF6")),
            LogTimelineMarkerKind.Search => new SolidColorBrush(Color.Parse("#3574F0")),
            LogTimelineMarkerKind.Bookmark => new SolidColorBrush(Color.Parse("#E0A800")),
            LogTimelineMarkerKind.Trigger => new SolidColorBrush(Color.Parse("#EF4444")),
            _ => Brushes.Gray
        };
    }
}

public sealed class LogTimelineJumpRequestedEventArgs : EventArgs
{
    public LogTimelineJumpRequestedEventArgs(long lineNumber)
    {
        LineNumber = lineNumber;
    }

    public long LineNumber { get; }
}
