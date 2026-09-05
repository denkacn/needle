using Avalonia.Controls;
using Avalonia.Interactivity;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Views;

public partial class LogTabsView : UserControl
{
    public LogTabsView()
    {
        InitializeComponent();
    }

    private void OnTabClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: LogTabViewModel tab } && DataContext is MainViewModel viewModel)
        {
            viewModel.SelectTabCommand.Execute(tab);
        }
    }

    private void OnCloseTabClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: LogTabViewModel tab } && DataContext is MainViewModel viewModel)
        {
            viewModel.CloseTabCommand.Execute(tab);
            e.Handled = true;
        }
    }
}
