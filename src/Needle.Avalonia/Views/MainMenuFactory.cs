namespace Needle.Avalonia.Views;

using global::Avalonia.Controls;
using Needle.Avalonia.ViewModels;

internal static class MainMenuFactory
{
    public static MenuFlyout Create(MainMenuActionSet actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        return new MenuFlyout
        {
            ItemsSource = new Control[]
            {
                new MenuItem
                {
                    Header = "Open...",
                    Command = actions.ViewModel.OpenFileCommand
                },
                BuildRecentFilesMenu(actions.ViewModel),
                new MenuItem
                {
                    Header = "Close Tab",
                    Command = actions.ViewModel.CloseSelectedTabCommand
                },
                new MenuItem
                {
                    Header = "About Needle",
                    Command = actions.ShowAboutCommand
                }
            }
        };
    }

    private static MenuItem BuildRecentFilesMenu(MainViewModel viewModel)
    {
        var recentFilesMenu = new MenuItem
        {
            Header = "Recent Files",
            IsEnabled = viewModel.RecentFiles.Count > 0
        };

        recentFilesMenu.ItemsSource = viewModel.RecentFiles.Count == 0
            ? [new MenuItem { Header = "No recent files", IsEnabled = false }]
            : viewModel.RecentFiles.Select(CreateRecentFileMenuItem).ToArray();

        return recentFilesMenu;
    }

    private static MenuItem CreateRecentFileMenuItem(RecentFileViewModel file)
    {
        var item = new MenuItem
        {
            Header = file.DisplayName,
            Command = file.OpenCommand
        };

        ToolTip.SetTip(item, file.Path);
        return item;
    }
}

internal sealed class MainMenuActionSet
{
    public MainMenuActionSet(MainViewModel viewModel, Action showAbout)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        ShowAboutCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(showAbout);
    }

    public MainViewModel ViewModel { get; }

    public CommunityToolkit.Mvvm.Input.IRelayCommand ShowAboutCommand { get; }
}
