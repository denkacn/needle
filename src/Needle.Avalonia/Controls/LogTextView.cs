namespace Needle.Avalonia.Controls;

using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Input.Platform;
using global::Avalonia.Media;
using Needle.Avalonia.ViewModels;

public sealed class LogTextView : Control
{
    public static readonly DirectProperty<LogTextView, IEnumerable?> LinesProperty =
        AvaloniaProperty.RegisterDirect<LogTextView, IEnumerable?>(
            nameof(Lines),
            view => view.Lines,
            (view, value) => view.Lines = value);

    public static readonly StyledProperty<double> LineHeightProperty =
        AvaloniaProperty.Register<LogTextView, double>(nameof(LineHeight), 22);

    public static readonly StyledProperty<double> GutterWidthProperty =
        AvaloniaProperty.Register<LogTextView, double>(nameof(GutterWidth), 80);

    public static readonly StyledProperty<double> TextFontSizeProperty =
        AvaloniaProperty.Register<LogTextView, double>(nameof(TextFontSize), 13);

    public static readonly StyledProperty<FontFamily> TextFontFamilyProperty =
        AvaloniaProperty.Register<LogTextView, FontFamily>(
            nameof(TextFontFamily),
            new FontFamily("Cascadia Mono,Consolas,Menlo,monospace"));

    public static readonly StyledProperty<IBrush?> TextBrushProperty =
        AvaloniaProperty.Register<LogTextView, IBrush?>(nameof(TextBrush), Brushes.Black);

    public static readonly StyledProperty<IBrush?> LineNumberBrushProperty =
        AvaloniaProperty.Register<LogTextView, IBrush?>(nameof(LineNumberBrush), Brushes.Gray);

    public static readonly StyledProperty<IBrush?> SelectedLineBrushProperty =
        AvaloniaProperty.Register<LogTextView, IBrush?>(nameof(SelectedLineBrush), Brushes.SteelBlue);

    public static readonly StyledProperty<IBrush?> TextSelectionBrushProperty =
        AvaloniaProperty.Register<LogTextView, IBrush?>(nameof(TextSelectionBrush), Brushes.SteelBlue);

    public static readonly StyledProperty<IBrush?> BookmarkBackgroundBrushProperty =
        AvaloniaProperty.Register<LogTextView, IBrush?>(nameof(BookmarkBackgroundBrush), Brushes.Transparent);

    public static readonly StyledProperty<IBrush?> TriggerBackgroundBrushProperty =
        AvaloniaProperty.Register<LogTextView, IBrush?>(nameof(TriggerBackgroundBrush), new SolidColorBrush(Color.Parse("#4A1F24")));

    public static readonly StyledProperty<IBrush?> GutterBrushProperty =
        AvaloniaProperty.Register<LogTextView, IBrush?>(nameof(GutterBrush), Brushes.Transparent);

    public static readonly StyledProperty<long> SelectedLineNumberProperty =
        AvaloniaProperty.Register<LogTextView, long>(
            nameof(SelectedLineNumber),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<string> SelectedTextProperty =
        AvaloniaProperty.Register<LogTextView, string>(
            nameof(SelectedText),
            string.Empty,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<ICommand?> ToggleBookmarkCommandProperty =
        AvaloniaProperty.Register<LogTextView, ICommand?>(nameof(ToggleBookmarkCommand));

    private IEnumerable? _lines;
    private readonly LogTextPointerController _pointer = new();
    private readonly LogTextRenderer _renderer = new();
    private readonly LogTextHitTester _hitTester = new();

    static LogTextView()
    {
        AffectsMeasure<LogTextView>(
            LineHeightProperty,
            GutterWidthProperty,
            TextFontSizeProperty,
            TextFontFamilyProperty);

        AffectsRender<LogTextView>(
            LineHeightProperty,
            GutterWidthProperty,
            TextFontSizeProperty,
            TextFontFamilyProperty,
            TextBrushProperty,
            LineNumberBrushProperty,
            SelectedLineBrushProperty,
            TextSelectionBrushProperty,
            BookmarkBackgroundBrushProperty,
            TriggerBackgroundBrushProperty,
            GutterBrushProperty,
            SelectedLineNumberProperty,
            SelectedTextProperty);
    }

    public LogTextView()
    {
        Focusable = true;
        ContextMenu = LogTextContextMenuFactory.Create(this);
    }

    public IEnumerable? Lines
    {
        get => _lines;
        set
        {
            if (ReferenceEquals(_lines, value))
            {
                return;
            }

            if (_lines is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnLinesCollectionChanged;
            }

            SetAndRaise(LinesProperty, ref _lines, value);

            if (_lines is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnLinesCollectionChanged;
            }

            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    public double GutterWidth
    {
        get => GetValue(GutterWidthProperty);
        set => SetValue(GutterWidthProperty, value);
    }

    public double TextFontSize
    {
        get => GetValue(TextFontSizeProperty);
        set => SetValue(TextFontSizeProperty, value);
    }

    public FontFamily TextFontFamily
    {
        get => GetValue(TextFontFamilyProperty);
        set => SetValue(TextFontFamilyProperty, value);
    }

    public IBrush? TextBrush
    {
        get => GetValue(TextBrushProperty);
        set => SetValue(TextBrushProperty, value);
    }

    public IBrush? LineNumberBrush
    {
        get => GetValue(LineNumberBrushProperty);
        set => SetValue(LineNumberBrushProperty, value);
    }

    public IBrush? SelectedLineBrush
    {
        get => GetValue(SelectedLineBrushProperty);
        set => SetValue(SelectedLineBrushProperty, value);
    }

    public IBrush? TextSelectionBrush
    {
        get => GetValue(TextSelectionBrushProperty);
        set => SetValue(TextSelectionBrushProperty, value);
    }

    public IBrush? BookmarkBackgroundBrush
    {
        get => GetValue(BookmarkBackgroundBrushProperty);
        set => SetValue(BookmarkBackgroundBrushProperty, value);
    }

    public IBrush? TriggerBackgroundBrush
    {
        get => GetValue(TriggerBackgroundBrushProperty);
        set => SetValue(TriggerBackgroundBrushProperty, value);
    }

    public IBrush? GutterBrush
    {
        get => GetValue(GutterBrushProperty);
        set => SetValue(GutterBrushProperty, value);
    }

    public long SelectedLineNumber
    {
        get => GetValue(SelectedLineNumberProperty);
        set => SetValue(SelectedLineNumberProperty, value);
    }

    public string SelectedText
    {
        get => GetValue(SelectedTextProperty);
        set => SetValue(SelectedTextProperty, value);
    }

    public ICommand? ToggleBookmarkCommand
    {
        get => GetValue(ToggleBookmarkCommandProperty);
        set => SetValue(ToggleBookmarkCommandProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        _renderer.Render(
            context,
            Lines,
            _pointer.Selection,
            new LogTextRenderSettings(
                LogTextMetrics.ToFiniteWidth(Bounds.Width),
                Bounds.Height,
                LineHeight,
                GutterWidth,
                TextFontSize,
                TextFontFamily,
                TextBrush ?? Brushes.Black,
                LineNumberBrush ?? Brushes.Gray,
                SelectedLineBrush ?? Brushes.SteelBlue,
                TextSelectionBrush ?? Brushes.SteelBlue,
                BookmarkBackgroundBrush ?? Brushes.Transparent,
                TriggerBackgroundBrush ?? new SolidColorBrush(Color.Parse("#4A1F24")),
                GutterBrush,
                SelectedLineNumber));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return LogTextMetrics.EstimateDesiredSize(
            Lines,
            availableSize,
            LineHeight,
            GutterWidth,
            TextFontSize,
            TextFontFamily);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var pointerPoint = e.GetCurrentPoint(this);
        var position = e.GetPosition(this);
        if (pointerPoint.Properties.IsLeftButtonPressed && TryToggleSelectedLineBookmark(position))
        {
            e.Handled = true;
            return;
        }

        SelectAt(position, pointerPoint.Properties.IsLeftButtonPressed, e.Pointer);
        if (pointerPoint.Properties.IsLeftButtonPressed)
        {
            e.Handled = true;
        }
    }

    public bool SelectAt(Point position, bool beginTextSelection, IPointer? pointer = null)
    {
        Focus();
        return _pointer.SelectAt(CreatePointerContext(), position, beginTextSelection, pointer);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        e.Handled = _pointer.Move(CreatePointerContext(), e.GetPosition(this));
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        e.Handled = _pointer.Release(CreatePointerContext(), e.GetPosition(this), e.Pointer);
    }

    private void OnLinesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateMeasure();
        InvalidateVisual();
    }

    private bool TryToggleSelectedLineBookmark(Point position)
    {
        if (position.X is < 6 or > 24 || ToggleBookmarkCommand is not { } command)
        {
            return false;
        }

        var line = _hitTester.GetLineAt(Lines, position.Y, LineHeight);
        if (line is null || line.LineNumber != SelectedLineNumber || !command.CanExecute(null))
        {
            return false;
        }

        command.Execute(null);
        InvalidateVisual();
        return true;
    }

    private LogTextPointerContext CreatePointerContext()
    {
        return new LogTextPointerContext(
            Lines,
            LineHeight,
            GutterWidth,
            TextFontSize,
            TextFontFamily,
            this,
            value => SelectedLineNumber = value,
            value => SelectedText = value,
            InvalidateVisual);
    }

    internal async Task CopySelectionAsync(IClipboard? clipboard)
    {
        await LogTextClipboardController.CopySelectionAsync(_pointer.Selection, SelectedText, clipboard);
    }

}
