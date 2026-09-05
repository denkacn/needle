using Needle.Avalonia.Services;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Tests;

public sealed class ExternalFileActionCoordinatorTests
{
    [Fact]
    public void CanRunReturnsFalseWhenPathIsMissing()
    {
        var coordinator = new ExternalFileActionCoordinator(new FakeExternalFileLauncher());

        Assert.False(coordinator.CanRun(isDocumentOpen: true, "D:\\Logs\\missing.log"));
        Assert.Equal(LogStatusFormatter.ActiveFileNotFound, coordinator.OpenInDefaultEditor(isDocumentOpen: true, "D:\\Logs\\missing.log"));
    }

    [Fact]
    public void OpenInDefaultEditorRunsLauncherWhenFileExists()
    {
        using var tempFile = new TempLogFile();
        var launcher = new FakeExternalFileLauncher();
        var coordinator = new ExternalFileActionCoordinator(launcher);

        var status = coordinator.OpenInDefaultEditor(isDocumentOpen: true, tempFile.Path);

        Assert.Null(status);
        Assert.Equal(tempFile.Path, launcher.OpenedPath);
    }

    private sealed class FakeExternalFileLauncher : IExternalFileLauncher
    {
        public string? OpenedPath { get; private set; }

        public void ShowInFileManager(string path)
        {
        }

        public void OpenInDefaultEditor(string path)
        {
            OpenedPath = path;
        }
    }

    private sealed class TempLogFile : IDisposable
    {
        public TempLogFile()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
            File.WriteAllText(Path, "test");
        }

        public string Path { get; }

        public void Dispose()
        {
            File.Delete(Path);
        }
    }
}
