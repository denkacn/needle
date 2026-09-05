using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;

namespace Needle.Avalonia;

public partial class App : global::Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var (mainWindow, viewModel) = AppCompositionRoot.CreateMainWindow();
            Task.Run(viewModel.RestoreStartupPreferencesAsync).GetAwaiter().GetResult();
            mainWindow.DataContext = viewModel;
            desktop.MainWindow = mainWindow;
            Dispatcher.UIThread.Post(async () => await viewModel.CheckForUpdatesAsync());

            var path = desktop.Args?.FirstOrDefault(File.Exists);
            if (path is not null)
            {
                Dispatcher.UIThread.Post(async () => await viewModel.OpenFilePathAsync(path));
            }
            else
            {
                Dispatcher.UIThread.Post(async () => await viewModel.RestoreWorkspaceAsync());
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
