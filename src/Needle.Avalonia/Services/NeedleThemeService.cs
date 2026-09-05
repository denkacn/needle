namespace Needle.Avalonia.Services;

using global::Avalonia.Controls;
using global::Avalonia.Media;

internal sealed class NeedleThemeService : INeedleThemeService
{
    private static readonly NeedleThemePalette Dark = new(
        Window: "#1F2023",
        TopBar: "#2B2D30",
        TabBar: "#232528",
        Panel: "#242629",
        Border: "#373A40",
        Control: "#1E1F22",
        Editor: "#1E1F22",
        Hover: "#383B40",
        ThemeButton: "#34373D",
        ThemeButtonHover: "#41454D",
        TabActive: "#26282C",
        Flyout: "#25272B",
        FlyoutSecondary: "#2B2D30",
        FlyoutPrimaryHover: "#467FF2",
        LogSelectedLine: "#264F78",
        LogTextSelection: "#3A6EA5",
        LogBookmark: "#2B2412",
        SectionActive: "#2B2D30",
        Item: "#232528",
        Text: "#C9D1D9",
        MutedText: "#8B949E",
        Accent: "#3574F0",
        DefaultLogText: "#C2CBD6");

    private static readonly NeedleThemePalette Light = new(
        Window: "#F4F5F7",
        TopBar: "#E7E9ED",
        TabBar: "#ECEEF2",
        Panel: "#EEF0F3",
        Border: "#CDD2DA",
        Control: "#FFFFFF",
        Editor: "#FFFFFF",
        Hover: "#DDE2EA",
        ThemeButton: "#DDE3EC",
        ThemeButtonHover: "#D1D8E3",
        TabActive: "#FFFFFF",
        Flyout: "#FFFFFF",
        FlyoutSecondary: "#EEF1F5",
        FlyoutPrimaryHover: "#2F69DC",
        LogSelectedLine: "#DCEBFF",
        LogTextSelection: "#BBD7FF",
        LogBookmark: "#FFF3C4",
        SectionActive: "#FFFFFF",
        Item: "#FFFFFF",
        Text: "#1F2328",
        MutedText: "#68707A",
        Accent: "#2563EB",
        DefaultLogText: "#24292F");

    private NeedleThemePalette _current = Dark;

    public bool IsDark { get; private set; } = true;

    public string DefaultLogTextColor => _current.DefaultLogText;

    public Color TextColor => Color.Parse(_current.Text);

    public Color MutedTextColor => Color.Parse(_current.MutedText);

    public Color SectionActiveColor => Color.Parse(_current.SectionActive);

    public void Apply(Control target, bool isDark)
    {
        ArgumentNullException.ThrowIfNull(target);

        IsDark = isDark;
        _current = isDark ? Dark : Light;

        SetBrush(target, "NeedleWindowBrush", _current.Window);
        SetBrush(target, "NeedleTopBarBrush", _current.TopBar);
        SetBrush(target, "NeedleTabBarBrush", _current.TabBar);
        SetBrush(target, "NeedlePanelBrush", _current.Panel);
        SetBrush(target, "NeedleBorderBrush", _current.Border);
        SetBrush(target, "NeedleControlBrush", _current.Control);
        SetBrush(target, "NeedleEditorBrush", _current.Editor);
        SetBrush(target, "NeedleHoverBrush", _current.Hover);
        SetBrush(target, "NeedleThemeButtonBrush", _current.ThemeButton);
        SetBrush(target, "NeedleThemeButtonHoverBrush", _current.ThemeButtonHover);
        SetBrush(target, "NeedleTabActiveBrush", _current.TabActive);
        SetBrush(target, "NeedleFlyoutBrush", _current.Flyout);
        SetBrush(target, "NeedleFlyoutSecondaryBrush", _current.FlyoutSecondary);
        SetBrush(target, "NeedleFlyoutPrimaryHoverBrush", _current.FlyoutPrimaryHover);
        SetBrush(target, "NeedleLogSelectedLineBrush", _current.LogSelectedLine);
        SetBrush(target, "NeedleLogTextSelectionBrush", _current.LogTextSelection);
        SetBrush(target, "NeedleLogBookmarkBrush", _current.LogBookmark);
        SetBrush(target, "NeedleSectionActiveBrush", _current.SectionActive);
        SetBrush(target, "NeedleItemBrush", _current.Item);
        SetBrush(target, "NeedleTextBrush", _current.Text);
        SetBrush(target, "NeedleMutedTextBrush", _current.MutedText);
        SetBrush(target, "NeedleAccentBrush", _current.Accent);
    }

    private static void SetBrush(Control target, string key, string color)
    {
        if (target.Resources.TryGetResource(key, null, out var resource)
            && resource is SolidColorBrush brush)
        {
            brush.Color = Color.Parse(color);
            return;
        }

        target.Resources[key] = new SolidColorBrush(Color.Parse(color));
    }
}
