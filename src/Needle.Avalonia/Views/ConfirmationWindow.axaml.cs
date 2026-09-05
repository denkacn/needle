using Avalonia.Interactivity;

namespace Needle.Avalonia.Views;

public partial class ConfirmationWindow : ChromeWindow
{
    public ConfirmationWindow()
        : this("Confirm", string.Empty, "OK")
    {
    }

    public ConfirmationWindow(string title, string message, string confirmText)
    {
        InitializeComponent();
        Title = title;
        MessageText.Text = message;
        ConfirmButton.Content = confirmText;
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void OnConfirmClicked(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
}
