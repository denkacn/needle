using Needle.Avalonia.Services;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Views;

public partial class TabTriggersWindow : ChromeWindow
{
    private readonly INeedleThemeService _themeService = new NeedleThemeService();

    public TabTriggersWindow()
        : this(new TriggerRulesViewModel())
    {
    }

    public TabTriggersWindow(TriggerRulesViewModel triggers)
    {
        InitializeComponent();
        DataContext = triggers;
    }

    public void ApplyTheme(bool isDarkTheme)
    {
        _themeService.Apply(this, isDarkTheme);
    }
}
