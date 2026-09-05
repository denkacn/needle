namespace Needle.Avalonia.Services;

using System.Diagnostics;

internal sealed class ExternalFileLauncher : IExternalFileLauncher
{
    public void ShowInFileManager(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        path = Path.GetFullPath(path);

        if (OperatingSystem.IsWindows())
        {
            StartProcess("explorer.exe", $"/select,\"{path}\"", useShellExecute: false);
            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            StartProcess("open", $"-R \"{path}\"", useShellExecute: false);
            return;
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            StartProcess("xdg-open", $"\"{directory}\"", useShellExecute: false);
        }
    }

    public void OpenInDefaultEditor(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        StartProcess(Path.GetFullPath(path), arguments: null, useShellExecute: true);
    }

    private static void StartProcess(string fileName, string? arguments, bool useShellExecute)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = useShellExecute
        };

        if (!string.IsNullOrWhiteSpace(arguments))
        {
            startInfo.Arguments = arguments;
        }

        Process.Start(startInfo);
    }
}
