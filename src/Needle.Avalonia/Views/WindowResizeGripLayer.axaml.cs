using Avalonia.Controls;
using Avalonia.Input;

namespace Needle.Avalonia.Views;

public partial class WindowResizeGripLayer : UserControl
{
    public WindowResizeGripLayer()
    {
        InitializeComponent();
    }

    private void OnResizeNorthPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TryBeginResizeDrag(WindowEdge.North, e);
    }

    private void OnResizeSouthPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TryBeginResizeDrag(WindowEdge.South, e);
    }

    private void OnResizeWestPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TryBeginResizeDrag(WindowEdge.West, e);
    }

    private void OnResizeEastPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TryBeginResizeDrag(WindowEdge.East, e);
    }

    private void OnResizeNorthWestPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TryBeginResizeDrag(WindowEdge.NorthWest, e);
    }

    private void OnResizeNorthEastPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TryBeginResizeDrag(WindowEdge.NorthEast, e);
    }

    private void OnResizeSouthWestPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TryBeginResizeDrag(WindowEdge.SouthWest, e);
    }

    private void OnResizeSouthEastPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TryBeginResizeDrag(WindowEdge.SouthEast, e);
    }

    private void TryBeginResizeDrag(WindowEdge edge, PointerPressedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            WindowChromeController.TryBeginResizeDrag(window, edge, e);
        }
    }
}
