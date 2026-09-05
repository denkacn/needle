namespace Needle.Infrastructure.Search;

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Needle.Application.Documents;
using Needle.Application.Search;
using Needle.Core.Documents;
using Needle.Core.Indexing;
using Needle.Core.Lines;
using Needle.Core.Search;

public sealed class IndexedLogSearchService : ILogSearchService
{
    private const int DefaultBatchSize = 512;

    private readonly ILineIndex _lineIndex;
    private readonly ILogDocumentReader _reader;
    private readonly int _batchSize;

    public IndexedLogSearchService(
        ILineIndex lineIndex,
        ILogDocumentReader reader,
        int batchSize = DefaultBatchSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
        _lineIndex = lineIndex ?? throw new ArgumentNullException(nameof(lineIndex));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _batchSize = batchSize;
    }

    public async IAsyncEnumerable<LogSearchMatch> SearchAsync(
        LogDocument document,
        LogSearchQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrEmpty(query.Pattern))
        {
            yield break;
        }

        var regex = query.IsRegex ? CreateRegex(query) : null;
        for (long startLine = 0; startLine < _lineIndex.LineCount; startLine += _batchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = (int)Math.Min(_batchSize, _lineIndex.LineCount - startLine);
            var entries = await _reader.GetLinesAsync(document, startLine, count, cancellationToken);

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var match in FindMatches(entry, query, regex))
                {
                    yield return match;
                }
            }
        }
    }

    private static Regex CreateRegex(LogSearchQuery query)
    {
        var options = RegexOptions.CultureInvariant;
        if (!query.CaseSensitive)
        {
            options |= RegexOptions.IgnoreCase;
        }

        return new Regex(query.Pattern, options, TimeSpan.FromSeconds(2));
    }

    private static IEnumerable<LogSearchMatch> FindMatches(
        LogEntry entry,
        LogSearchQuery query,
        Regex? regex)
    {
        if (regex is not null)
        {
            foreach (Match match in regex.Matches(entry.Text))
            {
                if (match.Success)
                {
                    yield return new LogSearchMatch(entry.Reference.LineNumber, match.Index, match.Length);
                }
            }

            yield break;
        }

        var comparison = query.CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        var startIndex = 0;
        while (startIndex <= entry.Text.Length)
        {
            var matchIndex = entry.Text.IndexOf(query.Pattern, startIndex, comparison);
            if (matchIndex < 0)
            {
                yield break;
            }

            yield return new LogSearchMatch(entry.Reference.LineNumber, matchIndex, query.Pattern.Length);
            startIndex = matchIndex + Math.Max(1, query.Pattern.Length);
        }
    }
}
