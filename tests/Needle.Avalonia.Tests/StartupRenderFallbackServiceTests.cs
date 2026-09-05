namespace Needle.Avalonia.Tests;

using Needle.Avalonia.Services;

public sealed class StartupRenderFallbackServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "Needle.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void ResolveUsesSoftwareRenderingWhenPreviousStartupMarkerExists()
    {
        var markerPath = CreateMarkerPath();
        File.WriteAllText(markerPath, "pending");
        var service = new StartupRenderFallbackService(markerPath);

        var decision = service.Resolve([]);

        Assert.True(decision.UseSoftwareRendering);
        Assert.Contains("previous startup", decision.Reason);
    }

    [Fact]
    public void ResolveUsesHardwareRenderingWhenCommandLineOverrideIsProvided()
    {
        var markerPath = CreateMarkerPath();
        File.WriteAllText(markerPath, "pending");
        var service = new StartupRenderFallbackService(markerPath);

        var decision = service.Resolve([StartupRenderFallbackService.ForceHardwareRenderingArgument]);

        Assert.False(decision.UseSoftwareRendering);
        Assert.Contains("override", decision.Reason);
    }

    [Fact]
    public void MarkStartupSucceededRemovesPendingMarker()
    {
        var markerPath = CreateMarkerPath();
        var service = new StartupRenderFallbackService(markerPath);

        service.MarkStartupPending(StartupRenderDecision.Software("test"));
        service.MarkStartupSucceeded();

        Assert.False(File.Exists(markerPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private string CreateMarkerPath()
    {
        Directory.CreateDirectory(_directory);
        return Path.Combine(_directory, "startup-render.pending");
    }
}
