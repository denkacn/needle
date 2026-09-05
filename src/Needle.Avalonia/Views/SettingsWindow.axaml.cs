using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Needle.Avalonia.Services;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Views;

public partial class SettingsWindow : ChromeWindow
{
    private readonly INeedleThemeService _themeService = new NeedleThemeService();
    private readonly WindowDialogService _dialogs;

    public SettingsWindow()
    {
        InitializeComponent();
        _dialogs = new WindowDialogService(this);
        ShowGeneralSection();
    }

    public void ApplyTheme(bool isDarkTheme)
    {
        _themeService.Apply(this, isDarkTheme);
        SetSectionButtonState(GeneralSectionButton, GeneralSection.IsVisible);
        SetSectionButtonState(HighlightsSectionButton, HighlightsSection.IsVisible);
    }

    private void OnWindowSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (WindowState != WindowState.Normal || DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.SaveSettingsWindowSize(Width, Height);
    }

    private void OnPickLogTextColorClicked(object? sender, RoutedEventArgs e)
    {
        AsyncEventRunner.Run(PickLogTextColorAsync);
    }

    private async Task PickLogTextColorAsync()
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var color = await PickColorAsync(viewModel.DisplaySettings.LogTextColor);
        if (color is not null)
        {
            viewModel.DisplaySettings.LogTextColor = color;
            viewModel.QueueSettingsSave();
        }
    }

    private void OnResetLogTextColorClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.DisplaySettings.ResetLogTextColor(_themeService.DefaultLogTextColor);
        viewModel.QueueSettingsSave();
    }

    private void OnPickTriggerHighlightColorClicked(object? sender, RoutedEventArgs e)
    {
        AsyncEventRunner.Run(PickTriggerHighlightColorAsync);
    }

    private async Task PickTriggerHighlightColorAsync()
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var color = await PickColorAsync(viewModel.DisplaySettings.TriggerHighlightColor);
        if (color is not null)
        {
            viewModel.DisplaySettings.TriggerHighlightColor = color;
            viewModel.QueueSettingsSave();
        }
    }

    private void OnResetTriggerHighlightColorClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.DisplaySettings.ResetTriggerHighlightColor();
        viewModel.QueueSettingsSave();
    }

    private void OnAddLogFontClicked(object? sender, RoutedEventArgs e)
    {
        AsyncEventRunner.Run(AddLogFontAsync);
    }

    private async Task AddLogFontAsync()
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var selectedFontName = await _dialogs.PickLogFontAsync(
            LogFontOptionsProvider.GetSystemFontNames(),
            viewModel.DisplaySettings.LogFontFamilyOptions);
        if (viewModel.DisplaySettings.AddCustomLogFont(selectedFontName))
        {
            viewModel.QueueSettingsSave();
        }
    }

    private void OnRemoveSelectedLogFontClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.DisplaySettings.RemoveSelectedCustomLogFont();
        viewModel.QueueSettingsSave();
    }
    private void OnExportSettingsClicked(object? sender, RoutedEventArgs e)
    {
        AsyncEventRunner.Run(ExportSettingsAsync);
    }

    private async Task ExportSettingsAsync()
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var path = await _dialogs.PickSettingsExportFileAsync();
        if (!string.IsNullOrWhiteSpace(path))
        {
            await viewModel.ExportSettingsAsync(path);
        }
    }

    private void OnImportSettingsClicked(object? sender, RoutedEventArgs e)
    {
        AsyncEventRunner.Run(ImportSettingsAsync);
    }

    private async Task ImportSettingsAsync()
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var path = await _dialogs.PickSettingsImportFileAsync();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var confirmed = await _dialogs.ConfirmAsync(
            "Import settings",
            "Current Needle settings will be replaced by the selected file.",
            "Import");
        if (confirmed)
        {
            await viewModel.ImportSettingsAsync(path);
        }
    }


    private void OnGeneralSectionClicked(object? sender, RoutedEventArgs e)
    {
        ShowGeneralSection();
    }

    private void OnHighlightsSectionClicked(object? sender, RoutedEventArgs e)
    {
        ShowHighlightsSection();
    }

    private void ShowGeneralSection()
    {
        GeneralSection.IsVisible = true;
        HighlightsSection.IsVisible = false;
        SetSectionButtonState(GeneralSectionButton, isActive: true);
        SetSectionButtonState(HighlightsSectionButton, isActive: false);
    }

    public void ShowHighlightsSection()
    {
        GeneralSection.IsVisible = false;
        HighlightsSection.IsVisible = true;
        SetSectionButtonState(GeneralSectionButton, isActive: false);
        SetSectionButtonState(HighlightsSectionButton, isActive: true);
    }

    private void SetSectionButtonState(Button button, bool isActive)
    {
        button.Background = isActive
            ? new SolidColorBrush(_themeService.SectionActiveColor)
            : Brushes.Transparent;
        button.Foreground = new SolidColorBrush(isActive ? _themeService.TextColor : _themeService.MutedTextColor);
    }

    private async Task<string?> PickColorAsync(string? initialColor)
    {
        return await _dialogs.PickColorAsync(initialColor);
    }

}


