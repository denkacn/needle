namespace Needle.Avalonia.Views;

using global::Avalonia.Controls;
using global::Avalonia.Input;

internal static class WindowChromeController
{
    public static void BeginMoveOrToggleMaximize(Window window, PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (!e.GetCurrentPoint(window).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleMaximized(window);
            return;
        }

        window.BeginMoveDrag(e);
    }

    public static void ToggleMaximized(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    public static void TryBeginResizeDrag(Window window, WindowEdge edge, PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (window.WindowState == WindowState.Maximized || !e.GetCurrentPoint(window).Properties.IsLeftButtonPressed)
        {
            return;
        }

        window.BeginResizeDrag(edge, e);
        e.Handled = true;
    }
}
