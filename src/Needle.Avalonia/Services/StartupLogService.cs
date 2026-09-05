namespace Needle.Avalonia.Services;

using System.Diagnostics;
using System.Runtime.InteropServices;

internal static class StartupLogService
{
    private static readonly object Gate = new();

    public static string LogPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Needle",
        "logs",
        "startup.log");

    public static void InstallGlobalHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            WriteException(e.ExceptionObject as Exception, "Unhandled domain exception");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            WriteException(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
    }

    public static void WriteStartup(string[] args)
    {
        Write(
            $"Needle startup. Args=[{string.Join(", ", args)}], " +
            $"OS={RuntimeInformation.OSDescription}, " +
            $"ProcessArchitecture={RuntimeInformation.ProcessArchitecture}, " +
            $"Framework={RuntimeInformation.FrameworkDescription}, " +
            $"ProcessPath={Environment.ProcessPath}");
    }

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                var directory = Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(LogPath, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    public static void WriteException(Exception? exception, string context)
    {
        Write(exception is null ? $"{context}: unknown exception" : $"{context}:{Environment.NewLine}{exception}");
    }
}
