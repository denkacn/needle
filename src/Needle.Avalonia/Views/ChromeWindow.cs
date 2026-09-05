using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Needle.Avalonia.Views;

public class ChromeWindow : Window
{
    protected void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        WindowChromeController.BeginMoveOrToggleMaximize(this, e);
    }

    protected void OnMinimizeClicked(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    protected void OnMaximizeClicked(object? sender, RoutedEventArgs e)
    {
        WindowChromeController.ToggleMaximized(this);
    }

    protected void OnCloseWindowClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
