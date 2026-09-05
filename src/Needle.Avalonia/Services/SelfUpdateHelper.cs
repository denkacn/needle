namespace Needle.Avalonia.Services;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

internal static class SelfUpdateHelper
{
    public const string ApplyUpdateArgument = "--needle-apply-update";

    public static bool TryRun(string[] args)
    {
        if (args.Length == 0 || !string.Equals(args[0], ApplyUpdateArgument, StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            Apply(args);
        }
        catch (Exception ex)
        {
            WriteLog($"Updater failed: {ex}");
            Environment.ExitCode = 1;
        }

        return true;
    }

    private static void Apply(string[] args)
    {
        var source = GetRequiredArgument(args, "--source");
        var target = GetRequiredArgument(args, "--target");
        var pid = int.Parse(GetRequiredArgument(args, "--pid"));
        var targetDirectory = Path.GetDirectoryName(target) ?? Environment.CurrentDirectory;
        var backup = $"{target}.bak";

        WriteLog($"Updater helper started. Source={source} Target={target} Process={pid}");
        WaitForProcessExit(pid);

        DeleteIfExists(backup);

        if (File.Exists(target))
        {
            InvokeWithRetry(() => File.Move(target, backup, true), "Move target to backup");
        }

        try
        {
            InvokeWithRetry(() => File.Copy(source, target, true), "Copy update to target");
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                WorkingDirectory = targetDirectory,
                UseShellExecute = true
            });
            WriteLog("Updated application started.");
            DeleteIfExists(backup);
        }
        catch
        {
            RestoreBackup(target, backup);
            throw;
        }
    }

    private static string GetRequiredArgument(string[] args, string name)
    {
        for (var i = 1; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.Ordinal))
            {
                return args[i + 1];
            }
        }

        throw new InvalidOperationException($"Missing update helper argument: {name}");
    }

    private static void WaitForProcessExit(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            WriteLog($"Waiting for process {processId} to exit.");
            process.WaitForExit();
        }
        catch (ArgumentException)
        {
            WriteLog($"Process {processId} is already closed.");
        }
    }

    private static void InvokeWithRetry(Action action, string name)
    {
        Exception? lastError = null;
        for (var attempt = 1; attempt <= 80; attempt++)
        {
            try
            {
                action();
                WriteLog($"{name} succeeded on attempt {attempt}.");
                return;
            }
            catch (Exception ex)
            {
                lastError = ex;
                WriteLog($"{name} failed on attempt {attempt}: {ex.Message}");
                Thread.Sleep(250);
            }
        }

        throw new IOException($"{name} failed after retries.", lastError);
    }

    private static void RestoreBackup(string target, string backup)
    {
        if (!File.Exists(backup) || File.Exists(target))
        {
            return;
        }

        File.Move(backup, target, true);
        WriteLog("Backup restored.");
    }

    private static void DeleteIfExists(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            WriteLog($"Could not delete {path}: {ex.Message}");
        }
    }

    private static void WriteLog(string message)
    {
        try
        {
            var logDirectory = Path.Combine(Path.GetTempPath(), "Needle", "updates");
            Directory.CreateDirectory(logDirectory);
            File.AppendAllText(
                Path.Combine(logDirectory, "apply-update.log"),
                $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
        }
        catch
        {
        }
    }
}
