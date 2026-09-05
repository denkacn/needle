namespace Needle.Avalonia.Tests;

using Needle.Avalonia.ViewModels;

public sealed class LogSelectionCoordinatorTests
{
    [Fact]
    public void RefreshClearsSelectionWhenNoLineIsSelected()
    {
        var coordinator = new LogSelectionCoordinator();

        var state = coordinator.Refresh(0, [], "previous");

        Assert.Equal(string.Empty, state.SelectedLineText);
    }

    [Fact]
    public void RefreshUsesVisibleLineTextForSelectedLine()
    {
        var coordinator = new LogSelectionCoordinator();
        LogLineViewModel[] lines =
        [
            new(10, "ten", null, null, null, IsBookmarked: false, IsSelected: true)
        ];

        var state = coordinator.Refresh(10, lines, "previous");

        Assert.Equal("ten", state.SelectedLineText);
    }

    [Fact]
    public void GetTextToCopyPrefersSelectedText()
    {
        var coordinator = new LogSelectionCoordinator();

        Assert.Equal("part", coordinator.GetTextToCopy("part", "whole"));
        Assert.Equal("whole", coordinator.GetTextToCopy(string.Empty, "whole"));
        Assert.Null(coordinator.GetTextToCopy(string.Empty, string.Empty));
    }
}
