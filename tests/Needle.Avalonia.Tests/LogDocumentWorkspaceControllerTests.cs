namespace Needle.Avalonia.Tests;

using Needle.Application.Documents;
using Needle.Application.Filtering;
using Needle.Application.Search;
using Needle.Application.Tailing;
using Needle.Avalonia.ViewModels;
using Needle.Core.Documents;
using Needle.Core.Filtering;
using Needle.Core.Indexing;
using Needle.Core.Lines;
using Needle.Core.Search;
using Needle.Core.Sources;

public sealed class LogDocumentWorkspaceControllerTests
{
    [Fact]
    public void ActivateOpenedTabAddsTabMarksItActiveAndAttachesHighlights()
    {
        var attached = new List<LogTabViewModel>();
        var controller = CreateController(attach: attached.Add);
        var tab = CreateTab("first.log");

        controller.ActivateOpenedTab(tab.Path, tab);

        Assert.Single(controller.Tabs);
        Assert.True(controller.HasActiveTab);
        Assert.Same(tab, controller.ActiveTab);
        Assert.True(tab.IsActive);
        Assert.Same(tab, Assert.Single(attached));
    }

    [Fact]
    public void SwitchToChangesActiveDocumentAndVisualState()
    {
        var controller = CreateController();
        var first = CreateTab("first.log");
        var second = CreateTab("second.log");
        controller.ActivateOpenedTab(first.Path, first);
        controller.ActivateOpenedTab(second.Path, second);

        controller.SwitchTo(first);

        Assert.Same(first, controller.ActiveTab);
        Assert.True(first.IsActive);
        Assert.False(second.IsActive);
    }

    [Fact]
    public void CloseInactiveTabKeepsCurrentActiveTab()
    {
        var detached = new List<LogTabViewModel>();
        var controller = CreateController(detach: detached.Add);
        var first = CreateTab("first.log");
        var second = CreateTab("second.log");
        controller.ActivateOpenedTab(first.Path, first);
        controller.ActivateOpenedTab(second.Path, second);

        var result = controller.Close(first);

        Assert.False(result.WasActive);
        Assert.Null(result.NextActiveTab);
        Assert.Same(second, controller.ActiveTab);
        Assert.DoesNotContain(first, controller.Tabs);
        Assert.Same(first, Assert.Single(detached));
    }

    [Fact]
    public void CloseActiveTabReturnsNeighborForActivation()
    {
        var controller = CreateController();
        var first = CreateTab("first.log");
        var second = CreateTab("second.log");
        controller.ActivateOpenedTab(first.Path, first);
        controller.ActivateOpenedTab(second.Path, second);

        var result = controller.Close(second);

        Assert.True(result.WasActive);
        Assert.Same(first, result.NextActiveTab);
        Assert.DoesNotContain(second, controller.Tabs);
    }

    [Fact]
    public void ClearDropsActiveDocumentAndClearsVisualState()
    {
        var controller = CreateController();
        var tab = CreateTab("first.log");
        controller.ActivateOpenedTab(tab.Path, tab);

        controller.Clear();

        Assert.False(controller.HasActiveTab);
        Assert.Null(controller.ActiveTab);
        Assert.False(tab.IsActive);
    }

    private static LogDocumentWorkspaceController CreateController(
        Action<LogTabViewModel>? attach = null,
        Action<LogTabViewModel>? detach = null)
    {
        var controller = new LogDocumentWorkspaceController(new LogTabStateWorkflow());
        controller.SetTabLifecycleHandlers(attach ?? (_ => { }), detach ?? (_ => { }));
        return controller;
    }

    private static LogTabViewModel CreateTab(string path)
    {
        var document = new LogDocument(path, path, new FakeLogSource(path));
        var session = new LogDocumentSession(
            document,
            new FakeLineIndex(),
            new FakeDocumentReader(),
            new FakeSearchService(),
            new FakeFilterService(),
            new FakeTailService(),
            fileSize: 10);

        return new LogTabViewModel(path, session);
    }

    private sealed class FakeLogSource(string displayName) : ILogSource
    {
        public string DisplayName { get; } = displayName;

        public ValueTask<long> GetLengthAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(0L);

        public ValueTask<Stream> OpenReadAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult<Stream>(new MemoryStream());
    }

    private sealed class FakeLineIndex : ILineIndex
    {
        public long LineCount => 0;

        public bool TryGetLine(long lineNumber, out LogLineReference line)
        {
            line = default;
            return false;
        }
    }

    private sealed class FakeDocumentReader : ILogDocumentReader
    {
        public ValueTask<IReadOnlyList<LogEntry>> GetLinesAsync(
            LogDocument document,
            long startLine,
            int count,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<IReadOnlyList<LogEntry>>([]);
        }
    }

    private sealed class FakeSearchService : ILogSearchService
    {
        public async IAsyncEnumerable<LogSearchMatch> SearchAsync(
            LogDocument document,
            LogSearchQuery query,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    private sealed class FakeFilterService : ILogFilterService
    {
        public async IAsyncEnumerable<long> GetMatchingLineNumbersAsync(
            LogDocument document,
            LogFilter filter,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    private sealed class FakeTailService : ILogTailService
    {
        public ValueTask<TailUpdateResult> PollAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new TailUpdateResult(10, 10, 0, Truncated: false));
    }
}
