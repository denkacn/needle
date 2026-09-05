namespace Needle.Avalonia.ViewModels;

using Needle.Avalonia.Services;

internal sealed class ExternalFileActionCoordinator
{
    private readonly IExternalFileLauncher _launcher;

    public ExternalFileActionCoordinator(IExternalFileLauncher launcher)
    {
        _launcher = launcher;
    }

    public bool CanRun(bool isDocumentOpen, string? path)
    {
        return isDocumentOpen && !string.IsNullOrWhiteSpace(path) && File.Exists(path);
    }

    public string? ShowInFileManager(bool isDocumentOpen, string? path)
    {
        return Run(
            isDocumentOpen,
            path,
            _launcher.ShowInFileManager,
            "Show in Explorer failed");
    }

    public string? OpenInDefaultEditor(bool isDocumentOpen, string? path)
    {
        return Run(
            isDocumentOpen,
            path,
            _launcher.OpenInDefaultEditor,
            "Open in editor failed");
    }

    private static string? Run(bool isDocumentOpen, string? path, Action<string> action, string failurePrefix)
    {
        if (!isDocumentOpen || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return LogStatusFormatter.ActiveFileNotFound;
        }

        try
        {
            action(path);
            return null;
        }
        catch (Exception ex)
        {
            return LogStatusFormatter.ExternalActionFailed(failurePrefix, ex);
        }
    }
}
