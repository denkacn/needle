using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Tests;

public sealed class LogStatusFormatterTests
{
    [Fact]
    public void ViewportIncludesVisibleCountOnlyWhenVisibilityFilterIsActive()
    {
        var plain = LogStatusFormatter.Viewport(42, 2048, "1-20", isFilterActive: false, filteredLineCount: 7);
        var filtered = LogStatusFormatter.Viewport(42, 2048, "1-20", isFilterActive: true, filteredLineCount: 7);

        Assert.Equal("Lines: 42   Size: 2 KB   View: 1-20", plain);
        Assert.Equal("Lines: 42   Size: 2 KB   View: 1-20   Visible: 7", filtered);
    }

    [Fact]
    public void CopiedSelectionUsesLineNumber()
    {
        Assert.Equal("Copied selection from line 12", LogStatusFormatter.CopiedSelection(12));
    }
}
