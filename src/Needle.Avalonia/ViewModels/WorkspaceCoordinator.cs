namespace Needle.Avalonia.ViewModels;

using Needle.Application.Workspace;

internal sealed class WorkspaceCoordinator
{
    private readonly IWorkspaceStore? _store;
    private readonly DebouncedAsyncAction _save;
    private readonly Func<bool> _canSave;
    private readonly Func<WorkspaceState> _createState;

    public WorkspaceCoordinator(
        IWorkspaceStore? store,
        DebouncedAsyncAction save,
        Func<bool> canSave,
        Func<WorkspaceState> createState)
    {
        _store = store;
        _save = save;
        _canSave = canSave;
        _createState = createState;
    }

    public async Task<WorkspaceState?> LoadAsync()
    {
        if (_store is null)
        {
            return null;
        }

        try
        {
            return await _store.LoadAsync();
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public void QueueSave()
    {
        if (_store is null || !_canSave())
        {
            return;
        }

        _save.Queue(SaveAsync);
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_store is null)
        {
            return;
        }

        await _store.SaveAsync(_createState(), cancellationToken);
    }
}
