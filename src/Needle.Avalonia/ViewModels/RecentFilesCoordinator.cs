namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;

internal sealed class RecentFilesCoordinator
{
    private const int Limit = 10;
    private readonly Func<string, Task> _openAsync;

    public RecentFilesCoordinator(Func<string, Task> openAsync)
    {
        _openAsync = openAsync;
    }

    public ObservableCollection<RecentFileViewModel> Items { get; } = [];

    public IReadOnlyList<string> Paths => [.. Items.Select(file => file.Path)];

    public bool HasItems => Items.Count > 0;

    public void Add(string path)
    {
        path = NormalizePath(path);
        Remove(path);
        Items.Insert(0, CreateItem(path));

        while (Items.Count > Limit)
        {
            Items.RemoveAt(Items.Count - 1);
        }
    }

    public bool Remove(string path)
    {
        path = NormalizePath(path);
        var existing = Items.FirstOrDefault(file => string.Equals(file.Path, path, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            return false;
        }

        Items.Remove(existing);
        return true;
    }

    public void Restore(IEnumerable<string> paths)
    {
        Items.Clear();
        foreach (var path in paths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(NormalizePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(Limit))
        {
            Items.Add(CreateItem(path));
        }
    }

    private RecentFileViewModel CreateItem(string path)
    {
        return new RecentFileViewModel(path, _openAsync);
    }

    private static string NormalizePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return System.IO.Path.GetFullPath(path);
    }
}
