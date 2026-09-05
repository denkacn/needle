namespace Needle.Avalonia.Tests;

using Needle.Avalonia.ViewModels;
using Needle.Core.Search;

public sealed class LogSearchCoordinatorTests
{
    [Fact]
    public void GetPendingStatusReflectsEmptySearchText()
    {
        var coordinator = new LogSearchCoordinator();

        Assert.Equal(string.Empty, coordinator.GetPendingStatus(string.Empty));
        Assert.Equal("Press Next to search", coordinator.GetPendingStatus("error"));
    }

    [Fact]
    public void MatchNavigationWrapsAround()
    {
        var coordinator = new LogSearchCoordinator();
        LogSearchMatch[] matches =
        [
            new(10, 0, 5),
            new(20, 0, 5),
            new(30, 0, 5)
        ];

        Assert.Equal(matches[1], coordinator.GetNextMatch(matches, currentLine: 10));
        Assert.Equal(matches[0], coordinator.GetNextMatch(matches, currentLine: 30));
        Assert.Equal(matches[1], coordinator.GetPreviousMatch(matches, currentLine: 30));
        Assert.Equal(matches[2], coordinator.GetPreviousMatch(matches, currentLine: 10));
    }
}
