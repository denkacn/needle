using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Needle.Avalonia.Services;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Views;

public partial class MainWindow : ChromeWindow
{
    private readonly MainWindowThemeController _theme;
    private readonly ChildWindowManager _childWindows;
    private readonly WindowPlacementController _placement;
    private readonly StartupOverlayController _startupOverlay;
    private readonly WindowDialogService _dialogs;

    public MainWindow()
    {
        InitializeComponent();
        _theme = new MainWindowThemeController(this, new NeedleThemeService());
        _childWindows = new ChildWindowManager(this, () => _theme.IsDarkTheme);
        _placement = new WindowPlacementController(this);
        _startupOverlay = new StartupOverlayController(StartupOverlay);
        _dialogs = new WindowDialogService(this);
        _theme.Apply();
        UpdateThemeIcon();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnWindowSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        _placement.SaveIfNormal();
    }

    private void OnWindowPositionChanged(object? sender, PixelPointEventArgs e)
    {
        _placement.SaveIfNormal();
    }

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            _placement.Save();
            viewModel.SaveSettingsNowAsync().GetAwaiter().GetResult();
        }
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        StartupRenderFallbackService.Default.MarkStartupSucceeded();
        AsyncEventRunner.Run(_startupOverlay.FadeOutAsync);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _placement.Attach(DataContext as MainViewModel);
    }





    private void OnToggleThemeClicked(object? sender, RoutedEventArgs e)
    {
        _theme.Toggle(DataContext as MainViewModel, _childWindows.SettingsWindow);
        UpdateThemeIcon();
    }

    private void OnSettingsClicked(object? sender, RoutedEventArgs e)
    {
        ShowSettingsWindow(openHighlights: false);
    }

    private void OnHighlightSettingsClicked(object? sender, RoutedEventArgs e)
    {
        ShowTabHighlightWindow();
    }

    private void OnTriggerSettingsClicked(object? sender, RoutedEventArgs e)
    {
        _childWindows.ShowTabTriggers(DataContext as MainViewModel);
    }
    private void OnClearLogClicked(object? sender, RoutedEventArgs e)
    {
        AsyncEventRunner.Run(ClearLogAsync);
    }

    private async Task ClearLogAsync()
    {
        if (DataContext is not MainViewModel viewModel
            || !viewModel.ClearLogCommand.CanExecute(null))
        {
            return;
        }

        var confirmed = await _dialogs.ConfirmAsync(
            "Clear log file",
            "This will permanently delete all content from the current log file.",
            "Clear");
        if (confirmed)
        {
            await viewModel.ClearLogCommand.ExecuteAsync(null);
        }
    }


    private void OnMainMenuButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || DataContext is not MainViewModel viewModel)
        {
            return;
        }

        MainMenuFactory.Create(new MainMenuActionSet(viewModel, _childWindows.ShowAbout)).ShowAt(button);
    }

    private void ShowSettingsWindow(bool openHighlights)
    {
        _childWindows.ShowSettings(DataContext, openHighlights);
    }

    private void ShowTabHighlightWindow()
    {
        _childWindows.ShowTabHighlights(DataContext as MainViewModel);
    }

    private void UpdateThemeIcon()
    {
        ThemeIcon.Data = Geometry.Parse(_theme.IconData);
    }

}

