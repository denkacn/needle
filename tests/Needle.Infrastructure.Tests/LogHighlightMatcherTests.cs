namespace Needle.Infrastructure.Tests;

using Needle.Core.Highlighting;
using Needle.Application.Highlighting;

public sealed class LogHighlightMatcherTests
{
    [Fact]
    public void MatchesPlainTextCaseInsensitive()
    {
        var rule = new LogHighlightRule("ERROR", IsRegex: false, Foreground: null, Background: null);

        Assert.True(LogHighlightMatcher.IsMatch("error connection lost", rule));
    }

    [Fact]
    public void DoesNotMatchDisabledRule()
    {
        var rule = new LogHighlightRule("ERROR", IsRegex: false, Foreground: null, Background: null, Enabled: false);

        Assert.False(LogHighlightMatcher.IsMatch("ERROR connection lost", rule));
    }

    [Fact]
    public void MatchesRegexRule()
    {
        var rule = new LogHighlightRule(@"HTTP\s+5\d\d", IsRegex: true, Foreground: null, Background: null);

        Assert.True(LogHighlightMatcher.IsMatch("HTTP 503 backend unavailable", rule));
    }
}
