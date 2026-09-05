using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Needle.Avalonia.Controls;

public sealed class HueSliderControl : Control
{
    public static readonly StyledProperty<double> HueProperty =
        AvaloniaProperty.Register<HueSliderControl, double>(
            nameof(Hue),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    static HueSliderControl()
    {
        AffectsRender<HueSliderControl>(HueProperty);
    }

    public event EventHandler? HueChanged;

    public HueSliderControl()
    {
        Focusable = true;
    }

    public double Hue
    {
        get => GetValue(HueProperty);
        set => SetValue(HueProperty, NormalizeHue(value));
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var rect = new Rect(Bounds.Size);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        context.FillRectangle(
            new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Colors.Red, 0),
                    new GradientStop(Colors.Magenta, 1d / 6),
                    new GradientStop(Colors.Blue, 2d / 6),
                    new GradientStop(Colors.Cyan, 3d / 6),
                    new GradientStop(Colors.Lime, 4d / 6),
                    new GradientStop(Colors.Yellow, 5d / 6),
                    new GradientStop(Colors.Red, 1)
                }
            },
            rect);

        var y = (360 - Hue) / 360 * rect.Height;
        var left = new StreamGeometry();
        using (var geometry = left.Open())
        {
            geometry.BeginFigure(new Point(0, y), isFilled: true);
            geometry.LineTo(new Point(-7, y - 6));
            geometry.LineTo(new Point(-7, y + 6));
            geometry.EndFigure(isClosed: true);
        }

        var right = new StreamGeometry();
        using (var geometry = right.Open())
        {
            geometry.BeginFigure(new Point(rect.Width, y), isFilled: true);
            geometry.LineTo(new Point(rect.Width + 7, y - 6));
            geometry.LineTo(new Point(rect.Width + 7, y + 6));
            geometry.EndFigure(isClosed: true);
        }

        context.DrawGeometry(Brushes.White, new Pen(Brushes.Gray, 1), left);
        context.DrawGeometry(Brushes.White, new Pen(Brushes.Gray, 1), right);
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
        if (Bounds.Height <= 0)
        {
            return;
        }

        Hue = (1 - Math.Clamp(point.Y / Bounds.Height, 0, 1)) * 360;
        HueChanged?.Invoke(this, EventArgs.Empty);
    }

    private static double NormalizeHue(double hue)
    {
        hue %= 360;
        return hue < 0 ? hue + 360 : hue;
    }
}
