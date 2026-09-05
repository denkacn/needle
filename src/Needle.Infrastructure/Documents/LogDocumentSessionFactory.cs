namespace Needle.Infrastructure.Documents;

using Needle.Application.Documents;
using Needle.Core.Documents;
using Needle.Core.Indexing;
using Needle.Infrastructure.Encoding;
using Needle.Infrastructure.Filtering;
using Needle.Infrastructure.Indexing;
using Needle.Infrastructure.Search;
using Needle.Infrastructure.Sources;
using Needle.Infrastructure.Tailing;

public sealed class LogDocumentSessionFactory : ILogDocumentSessionFactory
{
    public async Task<LogDocumentSessionOpenResult> OpenAsync(
        string path,
        Action<LogDocumentSessionOpenProgress>? progress,
        CancellationToken cancellationToken)
    {
        var source = new FileLogSource(path);
        var document = new LogDocument(path, source.DisplayName, source);
        var size = await source.GetLengthAsync(cancellationToken);
        var encoding = await new EncodingDetector().DetectAsync(source, cancellationToken);
        progress?.Invoke(new LogDocumentSessionOpenProgress(source.DisplayName, size, encoding.DisplayName));

        var indexBuilder = new ChunkedLineIndexBuilder(encodingDetection: encoding);
        var index = (InMemoryLineIndex)await indexBuilder.BuildAsync(source, cancellationToken);
        var reader = new IndexedLogDocumentReader(index, encoding.Encoding);
        var searchService = new IndexedLogSearchService(index, reader);
        var filterService = new IndexedLogFilterService(index, reader);
        var tailService = new LogTailService(source, index, indexBuilder, size);
        var session = new LogDocumentSession(
            document,
            index,
            reader,
            searchService,
            filterService,
            tailService,
            size);

        return new LogDocumentSessionOpenResult(session, source.DisplayName, size, encoding.DisplayName);
    }
}
