namespace Needle.Application.Workspace;

public interface IWorkspaceStore
{
    ValueTask<WorkspaceState?> LoadAsync(CancellationToken cancellationToken = default);

    ValueTask SaveAsync(WorkspaceState state, CancellationToken cancellationToken = default);
}
