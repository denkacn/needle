using Needle.Application.Workspace;
using Needle.Infrastructure.Persistence;

namespace Needle.Infrastructure.Workspace;

public sealed class JsonWorkspaceStore : IWorkspaceStore
{
    private readonly JsonFileStore<WorkspaceState> _store;

    public JsonWorkspaceStore()
        : this(GetDefaultPath())
    {
    }

    public JsonWorkspaceStore(string path)
    {
        _store = new JsonFileStore<WorkspaceState>(path);
    }

    public static string GetDefaultPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "Needle", "workspace.json");
    }

    public async ValueTask<WorkspaceState?> LoadAsync(CancellationToken cancellationToken = default)
    {
        return await _store.LoadAsync(cancellationToken);
    }

    public async ValueTask SaveAsync(WorkspaceState state, CancellationToken cancellationToken = default)
    {
        await _store.SaveAsync(state, cancellationToken);
    }
}
