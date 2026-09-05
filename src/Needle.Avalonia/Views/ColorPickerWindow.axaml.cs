using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Needle.Avalonia.Views;

public partial class ColorPickerWindow : Window
{
    private bool _isUpdating;

    public ColorPickerWindow()
        : this("#CFE8FF")
    {
    }

    public ColorPickerWindow(string? initialColor)
    {
        InitializeComponent();
        SetColor(ParseColorOrDefault(initialColor));
    }

    public string SelectedColor { get; private set; } = "#CFE8FF";

    private void OnColorFieldChanged(object? sender, EventArgs e)
    {
        if (_isUpdating)
        {
            return;
        }

        SetColor(FromHsv(ColorField.Hue, ColorField.Saturation, ColorField.Value));
    }

    private void OnHueChanged(object? sender, EventArgs e)
    {
        if (_isUpdating)
        {
            return;
        }

        ColorField.Hue = HueSlider.Hue;
        SetColor(FromHsv(ColorField.Hue, ColorField.Saturation, ColorField.Value));
    }

    private void OnHexTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdating || !TryParseHex(HexText.Text, out var color))
        {
            return;
        }

        SetColor(color);
    }

    private void OnOkClicked(object? sender, RoutedEventArgs e)
    {
        Close(SelectedColor);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private void SetColor(Color color)
    {
        _isUpdating = true;
        try
        {
            var (hue, saturation, value) = ToHsv(color);
            SelectedColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            HueSlider.Hue = hue;
            ColorField.Hue = hue;
            ColorField.Saturation = saturation;
            ColorField.Value = value;
            HexText.Text = SelectedColor;
            PreviewSwatch.Background = new SolidColorBrush(color);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private static Color ParseColorOrDefault(string? value)
    {
        return TryParseHex(value, out var color) ? color : Color.Parse("#CFE8FF");
    }

    private static bool TryParseHex(string? value, out Color color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var text = value.Trim();
        if (!text.StartsWith('#'))
        {
            text = "#" + text;
        }

        if (text.Length != 7)
        {
            return false;
        }

        try
        {
            color = Color.Parse(text);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static (double Hue, double Saturation, double Value) ToHsv(Color color)
    {
        var r = color.R / 255d;
        var g = color.G / 255d;
        var b = color.B / 255d;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        var hue = delta == 0
            ? 0
            : max == r
                ? 60 * (((g - b) / delta) % 6)
                : max == g
                    ? 60 * (((b - r) / delta) + 2)
                    : 60 * (((r - g) / delta) + 4);

        if (hue < 0)
        {
            hue += 360;
        }

        var saturation = max == 0 ? 0 : delta / max;
        return (hue, saturation, max);
    }

    private static Color FromHsv(double hue, double saturation, double value)
    {
        hue %= 360;
        if (hue < 0)
        {
            hue += 360;
        }

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
