namespace Needle.Avalonia.Tests;

using Needle.Avalonia.Services;

public sealed class AssemblyAppInfoServiceTests
{
    [Fact]
    public void DisplayVersionUsesAssemblyInformationalVersion()
    {
        var service = new AssemblyAppInfoService();

        Assert.Equal("0.9.24", service.Version);
        Assert.Equal("v0.9.24", service.DisplayVersion);
    }
}
