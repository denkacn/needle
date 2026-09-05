namespace Needle.Avalonia.Tests;

using Needle.Avalonia.ViewModels;

public sealed class ExcludeRulesViewModelTests
{
    [Fact]
    public void AddRuleTrimsAndDeduplicatesPatterns()
    {
        var viewModel = new ExcludeRulesViewModel();
        var changeCount = 0;
        viewModel.RulesChanged += () => changeCount++;

        viewModel.PendingPattern = "  Heartbeat  ";
        viewModel.AddRuleCommand.Execute(null);
        viewModel.PendingPattern = "heartbeat";
        viewModel.AddRuleCommand.Execute(null);

        var rule = Assert.Single(viewModel.Rules);
        Assert.Equal("Heartbeat", rule.Pattern);
        Assert.True(viewModel.HasRules);
        Assert.Equal("1", viewModel.CountText);
        Assert.Equal("1 exclude rule", viewModel.Status);
        Assert.Equal(1, changeCount);
    }

    [Fact]
    public void RemoveRuleUpdatesStatusAndRaisesChange()
    {
        var viewModel = new ExcludeRulesViewModel();
        viewModel.Restore(["debug"]);
        var changeCount = 0;
        viewModel.RulesChanged += () => changeCount++;

        viewModel.Rules[0].RemoveCommand.Execute(null);

        Assert.Empty(viewModel.Rules);
        Assert.False(viewModel.HasRules);
        Assert.Equal(string.Empty, viewModel.Status);
        Assert.Equal(1, changeCount);
    }

    [Fact]
    public void RestoreRebuildsRulesWithoutRaisingChange()
    {
        var viewModel = new ExcludeRulesViewModel();
        var changeCount = 0;
        viewModel.RulesChanged += () => changeCount++;

        viewModel.Restore(["debug", "DEBUG", "trace"]);

        Assert.Equal(["debug", "trace"], viewModel.ToPatterns());
        Assert.Equal("2 exclude rules", viewModel.Status);
        Assert.Equal(0, changeCount);
    }
}
