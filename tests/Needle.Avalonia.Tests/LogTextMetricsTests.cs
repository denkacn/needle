using Avalonia;
using Avalonia.Media;
using Needle.Avalonia.Controls;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Tests;

public sealed class LogTextMetricsTests
{
    [Fact]
    public void EstimateDesiredSizeKeepsViewportWidthForShortLines()
    {
        var size = LogTextMetrics.EstimateDesiredSize(
            new[] { CreateLine("short") },
            new Size(500, 300),
            lineHeight: 22,
            gutterWidth: 80,
            textFontSize: 13,
            textFontFamily: new FontFamily("Consolas"));

        Assert.Equal(500, size.Width);
        Assert.Equal(22, size.Height);
    }

    [Fact]
    public void EstimateDesiredSizeExpandsForLongLines()
    {
        var size = LogTextMetrics.EstimateDesiredSize(
            new[] { CreateLine(new string('x', 300)) },
            new Size(500, 300),
            lineHeight: 22,
            gutterWidth: 80,
            textFontSize: 13,
            textFontFamily: new FontFamily("Consolas"));

        Assert.True(size.Width > 500);
    }

    private static LogLineViewModel CreateLine(string text)
    {
        return new LogLineViewModel(1, text, null, null, null, false, false);
    }
}
