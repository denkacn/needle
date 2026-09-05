namespace Needle.Avalonia.Views;

using System.Diagnostics;
using global::Avalonia.Interactivity;
using Needle.Avalonia.Services;

public partial class AboutWindow : ChromeWindow
{
    private const string WebsiteUrl = "https://needle.bypuziki.com/";
    private const string EmailUrl = "mailto:needle@bypuziki.com";

    public AboutWindow()
    {
        InitializeComponent();
        VersionText.Text = new AssemblyAppInfoService().DisplayVersion;
    }

    public void ApplyTheme(bool isDarkTheme)
    {
        new NeedleThemeService().Apply(this, isDarkTheme);
    }

    private void OnWebsiteClicked(object? sender, RoutedEventArgs e)
    {
        OpenUrl(WebsiteUrl);
    }

    private void OnEmailClicked(object? sender, RoutedEventArgs e)
    {
        OpenUrl(EmailUrl);
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StartupLogService.WriteException(ex, $"Failed to open URL '{url}'");
        }
    }
}
