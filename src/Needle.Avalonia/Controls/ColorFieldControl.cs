using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Needle.Avalonia.Controls;

public sealed class ColorFieldControl : Control
{
    public static readonly StyledProperty<double> HueProperty =
        AvaloniaProperty.Register<ColorFieldControl, double>(nameof(Hue));

    public static readonly StyledProperty<double> SaturationProperty =
        AvaloniaProperty.Register<ColorFieldControl, double>(
            nameof(Saturation),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<ColorFieldControl, double>(
            nameof(Value),
            1,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    static ColorFieldControl()
    {
        AffectsRender<ColorFieldControl>(HueProperty, SaturationProperty, ValueProperty);
    }

    public event EventHandler? ColorChanged;

    public ColorFieldControl()
    {
        Focusable = true;
    }

    public double Hue
    {
        get => GetValue(HueProperty);
        set => SetValue(HueProperty, NormalizeHue(value));
    }

    public double Saturation
    {
        get => GetValue(SaturationProperty);
        set => SetValue(SaturationProperty, Math.Clamp(value, 0, 1));
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, Math.Clamp(value, 0, 1));
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var rect = new Rect(Bounds.Size);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        context.FillRectangle(new SolidColorBrush(FromHsv(Hue, 1, 1)), rect);
        context.FillRectangle(
            new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Colors.White, 0),
                    new GradientStop(Color.FromArgb(0, 255, 255, 255), 1)
                }
            },
            rect);
        context.FillRectangle(
            new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.FromArgb(0, 0, 0, 0), 0),
                    new GradientStop(Colors.Black, 1)
                }
            },
            rect);

        var x = Saturation * rect.Width;
        var y = (1 - Value) * rect.Height;
        var marker = new Point(x, y);
        context.DrawEllipse(null, new Pen(Brushes.Black, 3), marker, 6, 6);
        context.DrawEllipse(null, new Pen(Brushes.White, 2), marker, 5, 5);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        e.Pointer.Capture(this);
        UpdateFromPoint(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Pointer.Captured != this)
        {
            return;
        }

        UpdateFromPoint(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.Pointer.Captured == this)
        {
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    private void UpdateFromPoint(Point point)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        Saturation = Math.Clamp(point.X / Bounds.Width, 0, 1);
        Value = 1 - Math.Clamp(point.Y / Bounds.Height, 0, 1);
        ColorChanged?.Invoke(this, EventArgs.Empty);
    }

    private static double NormalizeHue(double hue)
    {
        hue %= 360;
        return hue < 0 ? hue + 360 : hue;
    }

    private static Color FromHsv(double hue, double saturation, double value)
    {
        var chroma = value * saturation;
        var x = chroma * (1 - Math.Abs((hue / 60 % 2) - 1));
        var m = value - chroma;

        var (r, g, b) = hue switch
        {
            < 60 => (chroma, x, 0d),
            < 120 => (x, chroma, 0d),
            < 180 => (0d, chroma, x),
            < 240 => (0d, x, chroma),
            < 300 => (x, 0d, chroma),
            _ => (chroma, 0d, x)
        };

        return Color.FromRgb(
            (byte)Math.Round((r + m) * 255),
            (byte)Math.Round((g + m) * 255),
            (byte)Math.Round((b + m) * 255));
    }
}
