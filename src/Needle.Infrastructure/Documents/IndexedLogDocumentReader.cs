namespace Needle.Infrastructure.Documents;

using System.Text;
using Needle.Application.Documents;
using Needle.Core.Documents;
using Needle.Core.Indexing;
using Needle.Core.Lines;

public sealed class IndexedLogDocumentReader : ILogDocumentReader
{
    private readonly ILineIndex _lineIndex;
    private readonly Encoding _encoding;

    public IndexedLogDocumentReader(ILineIndex lineIndex, Encoding? encoding = null)
    {
        _lineIndex = lineIndex ?? throw new ArgumentNullException(nameof(lineIndex));
        _encoding = encoding ?? Encoding.UTF8;
    }

    public async ValueTask<IReadOnlyList<LogEntry>> GetLinesAsync(
        LogDocument document,
        long startLine,
        int count,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentOutOfRangeException.ThrowIfNegative(startLine);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (count == 0 || startLine >= _lineIndex.LineCount)
        {
            return Array.Empty<LogEntry>();
        }

        var references = new List<LogLineReference>(Math.Min(count, 1024));
        for (var i = 0; i < count; i++)
        {
            var lineNumber = startLine + i;
            if (!_lineIndex.TryGetLine(lineNumber, out var reference))
            {
                break;
            }

            references.Add(reference);
        }

        if (references.Count == 0)
        {
            return Array.Empty<LogEntry>();
        }

        var first = references[0];
        var last = references[^1];
        var startOffset = first.Offset;
        var endOffset = checked(last.Offset + last.Length);
        var rangeLength = checked((int)(endOffset - startOffset));
        var range = new byte[rangeLength];

        await using var stream = await document.Source.OpenReadAsync(cancellationToken);
        stream.Position = startOffset;
        var read = await stream.ReadAtLeastAsync(range, rangeLength, throwOnEndOfStream: false, cancellationToken);

        var result = new List<LogEntry>(references.Count);
        foreach (var reference in references)
        {
            var relativeOffset = checked((int)(reference.Offset - startOffset));
            var availableLength = Math.Max(0, Math.Min(reference.Length, read - relativeOffset));
            var text = availableLength == 0
                ? string.Empty
                : _encoding.GetString(range.AsSpan(relativeOffset, availableLength));

            result.Add(new LogEntry(reference, text));
        }

        return result;
    }
}
