namespace Needle.Avalonia.Tests;

using Needle.Avalonia.ViewModels;

public sealed class HighlightRulesViewModelTests
{
    [Fact]
    public void DefaultRulesContainOnlyCoreLogLevels()
    {
        var viewModel = new HighlightRulesViewModel();

        Assert.Equal(["FATAL", "ERROR", "WARNING", "INFO"], viewModel.Rules.Select(rule => rule.Pattern));
        Assert.True(viewModel.HasRules);
    }

    [Fact]
    public void TabSpecificRulesStartEmpty()
    {
        var viewModel = new HighlightRulesViewModel(includeDefaultRules: false);

        Assert.Empty(viewModel.Rules);
        Assert.False(viewModel.HasRules);
    }

    [Fact]
    public void AddUserRuleGivesItPriorityOverDefaultRules()
    {
        var viewModel = new HighlightRulesViewModel();

        viewModel.AddUserRule("custom", isRegex: false, foreground: "#CFE8FF", background: "#112233");

        Assert.Equal("custom", viewModel.Rules[0].Pattern);
        Assert.Equal("#CFE8FF", viewModel.Rules[0].Foreground);
        Assert.Equal("#112233", viewModel.Rules[0].Background);
    }

    [Fact]
    public void AddUserRuleCanHighlightTextWithoutBackground()
    {
        var viewModel = new HighlightRulesViewModel();

        viewModel.AddUserRule("custom", isRegex: false, foreground: "#CFE8FF", background: null);

        Assert.Equal("#CFE8FF", viewModel.Rules[0].Foreground);
        Assert.Null(viewModel.Rules[0].Background);
        Assert.Equal("Default", viewModel.Rules[0].BackgroundDisplay);
    }

    [Fact]
    public void ClearRemovesAllRules()
    {
        var viewModel = new HighlightRulesViewModel();

        viewModel.Clear();

        Assert.Empty(viewModel.Rules);
        Assert.Equal("Highlight rules cleared", viewModel.Status);
    }
}
