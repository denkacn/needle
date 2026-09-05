namespace Needle.Avalonia.Tests;

using Needle.Application.Documents;
using Needle.Application.Filtering;
using Needle.Application.Search;
using Needle.Application.Tailing;
using Needle.Avalonia.ViewModels;
using Needle.Core.Bookmarks;
using Needle.Core.Documents;
using Needle.Core.Filtering;
using Needle.Core.Indexing;
using Needle.Core.Lines;
using Needle.Core.Search;
using Needle.Core.Sources;

public sealed class LogTabRuntimeTests
{
    [Fact]
    public void ActiveDocumentTracksTabSessionAndPathTogether()
    {
        var active = new ActiveLogDocument();
        var tab = CreateTab("first.log", fileSize: 10);

        active.Activate(tab.Path, tab);

        Assert.True(active.HasTab);
        Assert.Same(tab, active.Tab);
        Assert.Same(tab.Session, active.Session);
        Assert.Equal(tab.Path, active.Path);
        Assert.True(active.IsActive(tab));

        active.Clear();

        Assert.False(active.HasTab);
        Assert.Null(active.Tab);
        Assert.Null(active.Session);
        Assert.Null(active.Path);
    }

    [Fact]
    public void RuntimeSavesTransientStateWithoutMixingUiState()
    {
        var tab = CreateTab("state.log", fileSize: 10);
        var state = new LogTabTransientState(
            FileSize: 42,
            ScrollLine: 12,
            SearchText: "error",
            IsSearchCaseSensitive: true,
            IsSearchRegex: false,
            SearchStatus: "1 match",
            SearchMatches: [new LogSearchMatch(7, 2, 5)],
            FilterText: "warn",
            FilterStatus: "filtered",
            IsFilterActive: true,
            FilteredLineNumbers: [7],
            ExcludePatterns: ["debug"],
            TriggerPatterns: ["panic"],
            ExcludeStatus: "1 exclude",
            Bookmarks: [new LogBookmark(5), new LogBookmark(2)],
            BookmarkStatus: "2",
            IsTailPaused: true,
            IsFollowingTail: false,
            IsStructuredMode: true,
            SelectedLineNumber: 7,
            SelectedLineText: "selected");

        tab.SaveTransientState(state);

        Assert.Equal(42, tab.FileSize);
        Assert.Equal(12, tab.ScrollLine);
        Assert.Equal("error", tab.SearchText);
        Assert.True(tab.IsSearchCaseSensitive);
        Assert.Equal("1 match", tab.SearchStatus);
        Assert.Equal([7], tab.FilteredLineNumbers);
        Assert.Equal(["debug"], tab.ExcludePatterns);
        Assert.Equal(["panic"], tab.TriggerPatterns);
        Assert.Equal([2, 5], tab.Bookmarks.Select(bookmark => bookmark.LineNumber));
        Assert.True(tab.IsTailPaused);
        Assert.False(tab.IsFollowingTail);
        Assert.True(tab.IsStructuredMode);
        Assert.Equal("selected", tab.SelectedLineText);
    }

    private static LogTabViewModel CreateTab(string path, long fileSize)
    {
        var document = new LogDocument(path, path, new FakeLogSource(path));
        var session = new LogDocumentSession(
            document,
            new FakeLineIndex(),
            new FakeDocumentReader(),
            new FakeSearchService(),
            new FakeFilterService(),
            new FakeTailService(fileSize),
            fileSize);

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

    private sealed class FakeTailService(long fileSize) : ILogTailService
    {
        public ValueTask<TailUpdateResult> PollAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new TailUpdateResult(fileSize, fileSize, 0, Truncated: false));
    }
}
