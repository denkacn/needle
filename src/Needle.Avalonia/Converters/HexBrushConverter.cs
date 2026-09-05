namespace Needle.Avalonia.Converters;

using System.Globalization;
using global::Avalonia.Data.Converters;
using global::Avalonia.Media;

public sealed class HexBrushConverter : IValueConverter
{
    public static readonly HexBrushConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var fallback = parameter as string ?? "#C2CBD6";
        var color = value as string;

        try
        {
            return new SolidColorBrush(Color.Parse(string.IsNullOrWhiteSpace(color) ? fallback : color));
        }
        catch (FormatException)
        {
            return new SolidColorBrush(Color.Parse(fallback));
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
