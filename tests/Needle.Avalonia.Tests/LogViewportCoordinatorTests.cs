namespace Needle.Avalonia.Tests;

using Needle.Avalonia.ViewModels;

public sealed class LogViewportCoordinatorTests
{
    [Fact]
    public void GetVisibleLineCountUsesFloorAndMinimum()
    {
        var coordinator = new LogViewportCoordinator();

        Assert.Equal(10, coordinator.GetVisibleLineCount(viewportHeight: 12, lineHeight: 22, minimumLineCount: 10));
        Assert.Equal(20, coordinator.GetVisibleLineCount(viewportHeight: 449, lineHeight: 22, minimumLineCount: 10));
    }

    [Fact]
    public void ClampFirstVisibleLineKeepsLineInsideScrollRange()
    {
        var coordinator = new LogViewportCoordinator();

        Assert.Equal(0, coordinator.ClampFirstVisibleLine(-10, maximumScrollLine: 50));
        Assert.Equal(25, coordinator.ClampFirstVisibleLine(25, maximumScrollLine: 50));
        Assert.Equal(50, coordinator.ClampFirstVisibleLine(80, maximumScrollLine: 50));
    }
}
