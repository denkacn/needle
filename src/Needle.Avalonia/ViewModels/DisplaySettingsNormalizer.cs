namespace Needle.Avalonia.ViewModels;

internal static class DisplaySettingsNormalizer
{
    public static double ClampWindowSize(double value, double minimum)
    {
        return double.IsFinite(value) ? Math.Max(minimum, value) : minimum;
    }

    public static bool IsValidPosition(double? value)
    {
        return value is not null && double.IsFinite(value.Value);
    }

    public static string NormalizeColor(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
