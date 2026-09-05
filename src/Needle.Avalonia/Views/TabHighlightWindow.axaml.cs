using Needle.Avalonia.Services;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Views;

public partial class TabHighlightWindow : ChromeWindow
{
    private readonly INeedleThemeService _themeService = new NeedleThemeService();

    public TabHighlightWindow()
    {
        InitializeComponent();
    }

    public TabHighlightWindow(HighlightRulesViewModel highlightRules)
        : this()
    {
        DataContext = highlightRules;
    }

    public void ApplyTheme(bool isDarkTheme)
    {
        _themeService.Apply(this, isDarkTheme);
    }

}


