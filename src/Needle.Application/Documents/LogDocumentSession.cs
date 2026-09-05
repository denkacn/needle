namespace Needle.Application.Documents;

using Needle.Application.Filtering;
using Needle.Application.Search;
using Needle.Application.Tailing;
using Needle.Core.Documents;
using Needle.Core.Indexing;

public sealed class LogDocumentSession
{
    public LogDocumentSession(
        LogDocument document,
        ILineIndex lineIndex,
        ILogDocumentReader reader,
        ILogSearchService searchService,
        ILogFilterService filterService,
        ILogTailService tailService,
        long fileSize)
    {
        Document = document;
        LineIndex = lineIndex;
        Reader = reader;
        SearchService = searchService;
        FilterService = filterService;
        TailService = tailService;
        FileSize = fileSize;
    }

    public LogDocument Document { get; }

    public ILineIndex LineIndex { get; }

    public ILogDocumentReader Reader { get; }

    public ILogSearchService SearchService { get; }

    public ILogFilterService FilterService { get; }

    public ILogTailService TailService { get; }

    public long FileSize { get; set; }

    public long LineCount => LineIndex.LineCount;
}
