using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Views;

public partial class AddLogFontWindow : Window
{
    public AddLogFontWindow()
    {
        InitializeComponent();
    }

    public AddLogFontWindow(IReadOnlyList<string> systemFontNames, IEnumerable<string> existingFontNames)
        : this()
    {
        DataContext = new AddLogFontViewModel(systemFontNames, existingFontNames);
    }

    private void OnOkClicked(object? sender, RoutedEventArgs e)
    {
        Close((DataContext as AddLogFontViewModel)?.SelectedFontName);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private void OnFontDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is AddLogFontViewModel { CanAccept: true } viewModel)
        {
            Close(viewModel.SelectedFontName);
        }
    }
}
