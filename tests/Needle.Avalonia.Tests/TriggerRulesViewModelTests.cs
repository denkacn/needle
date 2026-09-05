namespace Needle.Avalonia.Tests;

using Needle.Avalonia.ViewModels;
using Needle.Core.Lines;

public sealed class TriggerRulesViewModelTests
{
    [Fact]
    public void ScanNewLinesAddsMatchingHits()
    {
        var viewModel = new TriggerRulesViewModel();
        viewModel.RestoreRules(["panic"]);

        viewModel.ScanNewLines([
            new LogEntry(new LogLineReference(Offset: 0, Length: 10, LineNumber: 4), "everything is ok"),
            new LogEntry(new LogLineReference(Offset: 10, Length: 20, LineNumber: 5), "PANIC in worker")
        ]);

        var hit = Assert.Single(viewModel.Hits);
        Assert.Equal(5, hit.LineNumber);
        Assert.Equal("panic", hit.Pattern);
        Assert.True(viewModel.ContainsHit(5));
        Assert.True(viewModel.HasHits);
    }

    [Fact]
    public void ScanNewLinesDoesNotDuplicateSameLineAndPattern()
    {
        var viewModel = new TriggerRulesViewModel();
        viewModel.RestoreRules(["panic"]);
        var entry = new LogEntry(new LogLineReference(Offset: 0, Length: 20, LineNumber: 5), "panic");

        viewModel.ScanNewLines([entry]);
        viewModel.ScanNewLines([entry]);

        Assert.Single(viewModel.Hits);
    }

    [Fact]
    public void NavigationCommandsWrapAroundHits()
    {
        var navigated = new List<long>();
        var viewModel = new TriggerRulesViewModel();
        viewModel.NavigationRequested += navigated.Add;
        viewModel.RestoreRules(["panic"]);
        viewModel.ScanNewLines([
            new LogEntry(new LogLineReference(Offset: 0, Length: 20, LineNumber: 5), "panic"),
            new LogEntry(new LogLineReference(Offset: 20, Length: 20, LineNumber: 9), "panic")
        ]);

        viewModel.NextHitCommand.Execute(null);
        viewModel.NextHitCommand.Execute(null);

        Assert.Equal([5, 9], navigated);
    }
}
