namespace Needle.Infrastructure.Filtering;

using System.Runtime.CompilerServices;
using Needle.Application.Documents;
using Needle.Application.Filtering;
using Needle.Core.Documents;
using Needle.Core.Filtering;
using Needle.Core.Indexing;
using Needle.Core.Lines;

public sealed class IndexedLogFilterService : ILogFilterService
{
    private const int DefaultBatchSize = 512;

    private readonly ILineIndex _lineIndex;
    private readonly ILogDocumentReader _reader;
    private readonly int _batchSize;

    public IndexedLogFilterService(
        ILineIndex lineIndex,
        ILogDocumentReader reader,
        int batchSize = DefaultBatchSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
        _lineIndex = lineIndex ?? throw new ArgumentNullException(nameof(lineIndex));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _batchSize = batchSize;
    }

    public async IAsyncEnumerable<long> GetMatchingLineNumbersAsync(
        LogDocument document,
        LogFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(filter);

        if (filter.IncludePatterns.Count == 0 && filter.ExcludePatterns.Count == 0)
        {
            yield break;
        }

        for (long startLine = 0; startLine < _lineIndex.LineCount; startLine += _batchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = (int)Math.Min(_batchSize, _lineIndex.LineCount - startLine);
            var entries = await _reader.GetLinesAsync(document, startLine, count, cancellationToken);

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (Matches(entry, filter))
                {
                    yield return entry.Reference.LineNumber;
                }
            }
        }
    }

    private static bool Matches(LogEntry entry, LogFilter filter)
    {
        var text = entry.Text;

        if (filter.IncludePatterns.Count > 0 &&
            !filter.IncludePatterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return filter.ExcludePatterns.All(pattern => !text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}
