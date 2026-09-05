namespace Needle.Avalonia.ViewModels;

internal sealed class ActiveFileWorkflow
{
    private readonly ExternalFileActionCoordinator _externalFiles;
    private readonly ActiveLogClearWorkflow _clearLog = new();

    public ActiveFileWorkflow(ExternalFileActionCoordinator externalFiles)
    {
        _externalFiles = externalFiles;
    }

    public bool CanUse(bool isDocumentOpen, string? path)
    {
        return _externalFiles.CanRun(isDocumentOpen, path);
    }

    public void ShowInFileManager(ActiveFileActionContext context)
    {
        RunExternalAction(_externalFiles.ShowInFileManager(context.IsDocumentOpen, context.Path), context);
    }

    public void OpenInDefaultEditor(ActiveFileActionContext context)
    {
        RunExternalAction(_externalFiles.OpenInDefaultEditor(context.IsDocumentOpen, context.Path), context);
    }

    public Task ClearLogAsync(ActiveLogClearContext context)
    {
        return _clearLog.ClearAsync(context);
    }

    private static void RunExternalAction(string? failureStatus, ActiveFileActionContext context)
    {
        if (failureStatus is null)
        {
            return;
        }

        context.SetStatus(failureStatus);
        context.NotifyDocumentStateChanged();
    }
}

internal sealed record ActiveFileActionContext(
    bool IsDocumentOpen,
    string? Path,
    Action<string> SetStatus,
    Action NotifyDocumentStateChanged);
