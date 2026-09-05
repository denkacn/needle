namespace Needle.Avalonia.Views;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;

public partial class ChromeTitleBar : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ChromeTitleBar, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<bool> ShowMaximizeButtonProperty =
        AvaloniaProperty.Register<ChromeTitleBar, bool>(nameof(ShowMaximizeButton), true);

    public ChromeTitleBar()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool ShowMaximizeButton
    {
        get => GetValue(ShowMaximizeButtonProperty);
        set => SetValue(ShowMaximizeButtonProperty, value);
    }

    private Window? OwnerWindow => TopLevel.GetTopLevel(this) as Window;

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (OwnerWindow is { } window)
        {
            WindowChromeController.BeginMoveOrToggleMaximize(window, e);
        }
    }

    private void OnMinimizeClicked(object? sender, RoutedEventArgs e)
    {
        if (OwnerWindow is { } window)
        {
            window.WindowState = WindowState.Minimized;
        }
    }

    private void OnMaximizeClicked(object? sender, RoutedEventArgs e)
    {
        if (OwnerWindow is { } window)
        {
            WindowChromeController.ToggleMaximized(window);
        }
    }

    private void OnCloseWindowClicked(object? sender, RoutedEventArgs e)
    {
        OwnerWindow?.Close();
    }
}
