namespace Needle.Avalonia;

using global::Avalonia;
using Needle.Avalonia.Services;

internal sealed class ApplicationBootstrapper
{
    private readonly StartupRenderFallbackService _renderFallback;

    public ApplicationBootstrapper()
        : this(StartupRenderFallbackService.Default)
    {
    }

    internal ApplicationBootstrapper(StartupRenderFallbackService renderFallback)
    {
        _renderFallback = renderFallback ?? throw new ArgumentNullException(nameof(renderFallback));
    }

    public void Run(string[] args)
    {
        StartupLogService.InstallGlobalHandlers();
        StartupLogService.WriteStartup(args);

        try
        {
            if (SelfUpdateHelper.TryRun(args))
            {
                return;
            }

            var renderDecision = _renderFallback.Resolve(args);
            StartupLogService.Write(
                $"Startup render mode: {(renderDecision.UseSoftwareRendering ? "Software" : "Default")}. Reason={renderDecision.Reason}");
            _renderFallback.MarkStartupPending(renderDecision);

            BuildAvaloniaApp(renderDecision).StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            StartupLogService.WriteException(ex, "Fatal startup exception");
            throw;
        }
    }

    internal static AppBuilder BuildAvaloniaApp(StartupRenderDecision renderDecision = default)
    {
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

        if (OperatingSystem.IsWindows() && renderDecision.UseSoftwareRendering)
        {
            builder.With(new Win32PlatformOptions
            {
                RenderingMode = [Win32RenderingMode.Software]
            });
        }

        return builder;
    }
}
