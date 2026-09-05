namespace Needle.Avalonia.Services;

using System.Runtime.InteropServices;

internal sealed class StartupRenderFallbackService
{
    public const string ForceSoftwareRenderingArgument = "--software-rendering";
    public const string ForceHardwareRenderingArgument = "--hardware-rendering";

    public static StartupRenderFallbackService Default { get; } = new(
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Needle",
            "startup-render.pending"));

    private readonly string _startupMarkerPath;

    public StartupRenderFallbackService(string startupMarkerPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startupMarkerPath);
        _startupMarkerPath = startupMarkerPath;
    }

    public StartupRenderDecision Resolve(string[] args)
    {
        if (!OperatingSystem.IsWindows())
        {
            return StartupRenderDecision.Hardware("non-Windows platform");
        }

        if (HasArgument(args, ForceHardwareRenderingArgument))
        {
            return StartupRenderDecision.Hardware("command line override");
        }

        if (HasArgument(args, ForceSoftwareRenderingArgument))
        {
            return StartupRenderDecision.Software("command line override");
        }

        if (File.Exists(_startupMarkerPath))
        {
            return StartupRenderDecision.Software("previous startup did not reach main window");
        }

        if (RuntimeInformation.OSDescription.Contains("Server", StringComparison.OrdinalIgnoreCase))
        {
            return StartupRenderDecision.Software("Windows Server detected");
        }

        return StartupRenderDecision.Hardware("default");
    }

    public void MarkStartupPending(StartupRenderDecision decision)
    {
        try
        {
            var directory = Path.GetDirectoryName(_startupMarkerPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(
                _startupMarkerPath,
                $"StartedAt={DateTimeOffset.Now:O}{Environment.NewLine}UseSoftwareRendering={decision.UseSoftwareRendering}{Environment.NewLine}Reason={decision.Reason}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            StartupLogService.WriteException(ex, "Failed to write startup render marker");
        }
    }

    public void MarkStartupSucceeded()
    {
        try
        {
            if (File.Exists(_startupMarkerPath))
            {
                File.Delete(_startupMarkerPath);
            }
        }
        catch (Exception ex)
        {
            StartupLogService.WriteException(ex, "Failed to clear startup render marker");
        }
    }

    private static bool HasArgument(string[] args, string expected)
    {
        return args.Any(arg => string.Equals(arg, expected, StringComparison.OrdinalIgnoreCase));
    }
}

internal readonly record struct StartupRenderDecision(bool UseSoftwareRendering, string Reason)
{
    public static StartupRenderDecision Software(string reason)
    {
        return new StartupRenderDecision(true, reason);
    }

    public static StartupRenderDecision Hardware(string reason)
    {
        return new StartupRenderDecision(false, reason);
    }
}
