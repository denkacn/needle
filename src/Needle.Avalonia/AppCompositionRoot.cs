namespace Needle.Avalonia;

using Needle.Avalonia.ViewModels;
using Needle.Avalonia.Views;

internal static class AppCompositionRoot
{
    public static (MainWindow Window, MainViewModel ViewModel) CreateMainWindow()
    {
        var mainWindow = new MainWindow();
        var services = new AppServices(mainWindow);
        var viewModel = services.CreateMainViewModel();

        return (mainWindow, viewModel);
    }
}
