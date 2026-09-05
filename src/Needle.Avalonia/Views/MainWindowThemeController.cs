namespace Needle.Avalonia.Views;

using global::Avalonia.Controls;
using global::Avalonia.Styling;
using Needle.Avalonia.Services;
using Needle.Avalonia.ViewModels;

internal sealed class MainWindowThemeController
{
    public const string MoonIconData = "M12,3 C9.2,4.2 7.3,6.9 7.3,10 C7.3,14.4 10.9,18 15.3,18 C16.3,18 17.3,17.8 18.2,17.4 C16.8,19.6 14.3,21 11.5,21 C7.1,21 3.5,17.4 3.5,13 C3.5,8.4 7.1,4.6 12,3 Z";
    public const string SunIconData = "M12,7 A5,5 0,1 1,12,17 A5,5 0,1 1,12,7 M11,2 L13,2 L13,5 L11,5 Z M11,19 L13,19 L13,22 L11,22 Z M2,11 L5,11 L5,13 L2,13 Z M19,11 L22,11 L22,13 L19,13 Z M4.6,3.2 L7.1,5.7 L5.7,7.1 L3.2,4.6 Z M16.9,18.3 L18.3,16.9 L20.8,19.4 L19.4,20.8 Z M19.4,3.2 L20.8,4.6 L18.3,7.1 L16.9,5.7 Z M3.2,19.4 L5.7,16.9 L7.1,18.3 L4.6,20.8 Z";

    private readonly INeedleThemeService _themeService;
    private readonly Window _owner;
    private bool _isDarkTheme = true;

    public MainWindowThemeController(Window owner, INeedleThemeService themeService)
    {
        _owner = owner;
        _themeService = themeService;
    }

    public bool IsDarkTheme => _isDarkTheme;

    public string IconData => _isDarkTheme ? MoonIconData : SunIconData;

    public void Apply()
    {
        _themeService.Apply(_owner, _isDarkTheme);
    }

    public void Toggle(MainViewModel? viewModel, SettingsWindow? settingsWindow)
    {
        var previousDefaultColor = _themeService.DefaultLogTextColor;
        _isDarkTheme = !_isDarkTheme;
        global::Avalonia.Application.Current!.RequestedThemeVariant = _isDarkTheme
            ? ThemeVariant.Dark
            : ThemeVariant.Light;

        Apply();
        settingsWindow?.ApplyTheme(_isDarkTheme);
        ApplyDefaultLogTextColorForTheme(viewModel, previousDefaultColor);
    }

    private void ApplyDefaultLogTextColorForTheme(MainViewModel? viewModel, string previousDefaultColor)
    {
        if (viewModel is null)
        {
            return;
        }

        if (string.Equals(viewModel.DisplaySettings.LogTextColor, previousDefaultColor, StringComparison.OrdinalIgnoreCase)
            || string.Equals(viewModel.DisplaySettings.LogTextColor, _themeService.DefaultLogTextColor, StringComparison.OrdinalIgnoreCase))
        {
            viewModel.DisplaySettings.LogTextColor = _themeService.DefaultLogTextColor;
            viewModel.QueueSettingsSave();
        }
    }
}
