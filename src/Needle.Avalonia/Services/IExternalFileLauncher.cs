namespace Needle.Avalonia.Services;

public interface IExternalFileLauncher
{
    void ShowInFileManager(string path);

    void OpenInDefaultEditor(string path);
}
