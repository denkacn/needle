namespace Needle.Avalonia.ViewModels;

using global::Avalonia.Media;

internal static class LogFontOptionsProvider
{
    private const string FallbackFontFamily = "monospace";

    private static readonly string[] PreferredLogFonts =
    [
        "Cascadia Mono",
        "Consolas",
        "JetBrains Mono",
        "IBM Plex Mono",
        "Roboto Mono",
        "Source Code Pro",
        "Hack",
        "Inconsolata",
        "DejaVu Sans Mono",
        "Liberation Mono",
        "Ubuntu Mono",
        "Menlo",
        "Monaco",
        "SF Mono",
        "Courier New"
    ];

    public static IReadOnlyList<string> GetAvailableLogFonts()
    {
        return GetAvailableLogFonts([]);
    }

    public static IReadOnlyList<string> GetAvailableLogFonts(IEnumerable<string> customFontNames)
    {
        var installedFonts = GetInstalledFonts();
        if (installedFonts.Count == 0)
        {
            return [FallbackFontFamily];
        }

        var availableFonts = PreferredLogFonts.Concat(customFontNames)
            .Select(font => ResolveSystemFontName(font, installedFonts))
            .Where(font => !string.IsNullOrWhiteSpace(font))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToArray();

        return availableFonts.Length == 0 ? [FallbackFontFamily] : availableFonts;
    }

    public static IReadOnlyList<string> GetSystemFontNames()
    {
        return [.. GetInstalledFonts().Order(StringComparer.CurrentCultureIgnoreCase)];
    }

    public static string GetDefaultLogFont(IReadOnlyList<string> options)
    {
        return options.Count == 0 ? FallbackFontFamily : options[0];
    }

    public static string ResolveFontName(string? requestedFontName, IReadOnlyList<string> options)
    {
        if (!string.IsNullOrWhiteSpace(requestedFontName))
        {
            var match = options.FirstOrDefault(
                option => string.Equals(option, requestedFontName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                return match;
            }
        }

        return GetDefaultLogFont(options);
    }

    public static string? ResolveSystemFontName(string? requestedFontName)
    {
        return ResolveSystemFontName(requestedFontName, GetInstalledFonts());
    }

    public static IReadOnlyList<string> NormalizeCustomFonts(IEnumerable<string>? fontNames)
    {
        if (fontNames is null)
        {
            return [];
        }

        var installedFonts = GetInstalledFonts();
        return fontNames
            .Select(font => ResolveSystemFontName(font, installedFonts))
            .Where(font => !string.IsNullOrWhiteSpace(font))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .Order(StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private static HashSet<string> GetInstalledFonts()
    {
        try
        {
            return FontManager.Current.SystemFonts
                .Select(font => font.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return [];
        }
    }

    private static string? ResolveSystemFontName(string? requestedFontName, HashSet<string> installedFonts)
    {
        if (string.IsNullOrWhiteSpace(requestedFontName))
        {
            return null;
        }

        var trimmedFontName = requestedFontName.Trim();
        return installedFonts.FirstOrDefault(font => string.Equals(font, trimmedFontName, StringComparison.OrdinalIgnoreCase));
    }
}
