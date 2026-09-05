namespace Needle.Avalonia.Services;

using global::Avalonia.Controls;
using global::Avalonia.Media;

internal interface INeedleThemeService
{
    bool IsDark { get; }

    string DefaultLogTextColor { get; }

    Color TextColor { get; }

    Color MutedTextColor { get; }

    Color SectionActiveColor { get; }

    void Apply(Control target, bool isDark);
}
