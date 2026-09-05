using Avalonia.Controls;
using Needle.Avalonia.Services;
using Needle.Avalonia.ViewModels;
using Needle.Infrastructure.Documents;
using Needle.Infrastructure.Preferences;
using Needle.Infrastructure.Workspace;

namespace Needle.Avalonia;

internal sealed class AppServices
{
    private readonly Window _mainWindow;
    private readonly IAppInfoService _appInfo = new AssemblyAppInfoService();

    public AppServices(Window mainWindow)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
    }

    public MainViewModel CreateMainViewModel()
    {
        return new MainViewModel(
            new WindowLogFilePicker(_mainWindow),
            new JsonWorkspaceStore(),
            new JsonUserPreferencesStore(),
            _appInfo,
            new LogDocumentSessionFactory(),
            new ExternalFileLauncher(),
            new NetSparkleAppUpdateService(NetSparkleUpdateOptions.Default, _appInfo));
    }
}
