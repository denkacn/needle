namespace Needle.Avalonia.ViewModels;

using CommunityToolkit.Mvvm.Input;

public sealed class RecentFileViewModel
{
    public RecentFileViewModel(string path, Func<string, Task> openAsync)
    {
        Path = path;
        DisplayName = System.IO.Path.GetFileName(path);
        OpenCommand = new AsyncRelayCommand(() => openAsync(Path));
    }

    public string Path { get; }

    public string DisplayName { get; }

    public IAsyncRelayCommand OpenCommand { get; }
}
