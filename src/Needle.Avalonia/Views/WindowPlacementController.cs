using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Views;

internal sealed class WindowPlacementController
{
    private readonly Window _window;
    private readonly DispatcherTimer _saveTimer;
    private MainViewModel? _viewModel;

    public WindowPlacementController(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _saveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _saveTimer.Tick += OnSaveTimerTick;
    }

    public void Attach(MainViewModel? viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.DisplaySettings.MainWindowPlacementRestored -= OnMainWindowPlacementRestored;
        }

        _viewModel = viewModel;
        if (_viewModel is null)
        {
            return;
        }

        _viewModel.DisplaySettings.MainWindowPlacementRestored += OnMainWindowPlacementRestored;
        Apply();
    }

    public void SaveIfNormal()
    {
        if (_window.WindowState != WindowState.Normal)
        {
            _saveTimer.Stop();
            return;
        }

        _saveTimer.Stop();
        _saveTimer.Start();
    }

    public void Save()
    {
        _saveTimer.Stop();

        if (_viewModel is null)
        {
            return;
        }

        var bounds = new PixelRect(_window.Position, new PixelSize(
            Math.Max(1, (int)Math.Round(_window.Width)),
            Math.Max(1, (int)Math.Round(_window.Height))));

        if (!IsVisibleOnAnyScreen(bounds))
        {
            return;
        }

        _viewModel.SaveMainWindowPlacement(_window.Width, _window.Height, _window.Position.X, _window.Position.Y);
    }

    private void OnMainWindowPlacementRestored(object? sender, EventArgs e)
    {
        Apply();
    }

    private void Apply()
    {
        if (_viewModel is null)
        {
            return;
        }

        var width = _viewModel.DisplaySettings.MainWindowWidth;
        var height = _viewModel.DisplaySettings.MainWindowHeight;
        var workingArea = _window.Screens.ScreenFromPoint(_window.Position)?.WorkingArea
            ?? _window.Screens.Primary?.WorkingArea;
        if (workingArea is PixelRect area)
        {
            width = Math.Min(width, Math.Max(_window.MinWidth, area.Width * 0.95));
            height = Math.Min(height, Math.Max(_window.MinHeight, area.Height * 0.95));
        }

        _window.Width = width;
        _window.Height = height;

        if (!_viewModel.DisplaySettings.HasMainWindowPosition)
        {
            _window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            return;
        }

        var savedPosition = new PixelPoint(
            (int)Math.Round(_viewModel.DisplaySettings.MainWindowX),
            (int)Math.Round(_viewModel.DisplaySettings.MainWindowY));
        var savedBounds = new PixelRect(savedPosition, new PixelSize(
            Math.Max(1, (int)Math.Round(width)),
            Math.Max(1, (int)Math.Round(height))));

        if (!IsVisibleOnAnyScreen(savedBounds))
        {
            _window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            return;
        }

        _window.WindowStartupLocation = WindowStartupLocation.Manual;
        _window.Position = savedPosition;
    }

    private bool IsVisibleOnAnyScreen(PixelRect windowBounds)
    {
        foreach (var screen in _window.Screens.All)
        {
            if (windowBounds.Intersects(screen.WorkingArea))
            {
                return true;
            }
        }

        return false;
    }

    private void OnSaveTimerTick(object? sender, EventArgs e)
    {
        Save();
    }
}
