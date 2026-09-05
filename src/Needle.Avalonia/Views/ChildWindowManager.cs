using Avalonia.Controls;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Views;

internal sealed class ChildWindowManager
{
    private readonly Window _owner;
    private readonly Func<bool> _isDarkTheme;
    private SettingsWindow? _settingsWindow;
    private TabHighlightWindow? _tabHighlightWindow;
    private TabTriggersWindow? _tabTriggersWindow;
    private AboutWindow? _aboutWindow;

    public ChildWindowManager(Window owner, Func<bool> isDarkTheme)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _isDarkTheme = isDarkTheme ?? throw new ArgumentNullException(nameof(isDarkTheme));
    }

    public SettingsWindow? SettingsWindow => _settingsWindow;

    public void ShowSettings(object? dataContext, bool openHighlights)
    {
        if (_settingsWindow is { IsVisible: true })
        {
            if (openHighlights)
            {
                _settingsWindow.ShowHighlightsSection();
            }

            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow
        {
            DataContext = dataContext
        };
        _settingsWindow.ApplyTheme(_isDarkTheme());
        if (openHighlights)
        {
            _settingsWindow.ShowHighlightsSection();
        }

        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show(_owner);
    }

    public void ShowTabHighlights(MainViewModel? viewModel)
    {
        if (viewModel?.Document.SelectedTab is not LogTabViewModel tab)
        {
            return;
        }

        if (_tabHighlightWindow is { IsVisible: true })
        {
            _tabHighlightWindow.DataContext = tab.Highlights;
            _tabHighlightWindow.ApplyTheme(_isDarkTheme());
            _tabHighlightWindow.Activate();
            return;
        }

        _tabHighlightWindow = new TabHighlightWindow(tab.Highlights);
        _tabHighlightWindow.ApplyTheme(_isDarkTheme());
        _tabHighlightWindow.Closed += (_, _) => _tabHighlightWindow = null;
        _tabHighlightWindow.Show(_owner);
    }

    public void ShowTabTriggers(MainViewModel? viewModel)
    {
        if (viewModel?.Document.SelectedTab is not LogTabViewModel tab)
        {
            return;
        }

        if (_tabTriggersWindow is { IsVisible: true })
        {
            _tabTriggersWindow.DataContext = tab.Triggers;
            _tabTriggersWindow.ApplyTheme(_isDarkTheme());
            _tabTriggersWindow.Activate();
            return;
        }

        _tabTriggersWindow = new TabTriggersWindow(tab.Triggers);
        _tabTriggersWindow.ApplyTheme(_isDarkTheme());
        _tabTriggersWindow.Closed += (_, _) => _tabTriggersWindow = null;
        _tabTriggersWindow.Show(_owner);
    }

    public void ShowAbout()
    {
        if (_aboutWindow is { IsVisible: true })
        {
            _aboutWindow.ApplyTheme(_isDarkTheme());
            _aboutWindow.Activate();
            return;
        }

        _aboutWindow = new AboutWindow();
        _aboutWindow.ApplyTheme(_isDarkTheme());
        _aboutWindow.Closed += (_, _) => _aboutWindow = null;
        _aboutWindow.Show(_owner);
    }
}
