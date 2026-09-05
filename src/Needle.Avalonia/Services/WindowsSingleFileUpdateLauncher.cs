using System.Diagnostics;

namespace Needle.Avalonia.Services;

internal static class WindowsSingleFileUpdateLauncher
{
    public static void Start(string downloadedExecutablePath)
    {
        var currentExecutablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(currentExecutablePath))
        {
            currentExecutablePath = Process.GetCurrentProcess().MainModule?.FileName;
        }

        if (string.IsNullOrWhiteSpace(currentExecutablePath))
        {
            throw new InvalidOperationException("Cannot resolve the current executable path for self update.");
        }

        var updateDirectory = Path.GetDirectoryName(downloadedExecutablePath) ?? Path.GetTempPath();
        WriteUpdateLog(updateDirectory, $"Preparing updater. Source={downloadedExecutablePath} Target={currentExecutablePath}");

        var startInfo = new ProcessStartInfo
        {
            FileName = downloadedExecutablePath,
            CreateNoWindow = true,
            UseShellExecute = false,
            WorkingDirectory = updateDirectory
        };
        startInfo.ArgumentList.Add(SelfUpdateHelper.ApplyUpdateArgument);
        startInfo.ArgumentList.Add("--source");
        startInfo.ArgumentList.Add(downloadedExecutablePath);
        startInfo.ArgumentList.Add("--target");
        startInfo.ArgumentList.Add(currentExecutablePath);
        startInfo.ArgumentList.Add("--pid");
        startInfo.ArgumentList.Add(Environment.ProcessId.ToString());

        var process = Process.Start(startInfo);

        WriteUpdateLog(updateDirectory, process is null
            ? "Failed to start updater launcher process."
            : $"Updater launcher process started. Pid={process.Id}");
    }

    private static void WriteUpdateLog(string updateDirectory, string message)
    {
        try
        {
            var logDirectory = Path.Combine(Path.GetTempPath(), "Needle", "updates");
            Directory.CreateDirectory(logDirectory);
            File.AppendAllText(
                Path.Combine(logDirectory, "apply-update.log"),
                $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
            File.AppendAllText(
                Path.Combine(updateDirectory, "apply-update.log"),
                $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
        }
        catch
        {
        }
    }
}
