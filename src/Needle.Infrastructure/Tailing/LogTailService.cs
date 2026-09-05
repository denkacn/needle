namespace Needle.Infrastructure.Tailing;

using Needle.Application.Tailing;
using Needle.Core.Indexing;
using Needle.Core.Lines;
using Needle.Core.Sources;
using Needle.Infrastructure.Indexing;
using Needle.Infrastructure.Sources;

public sealed class LogTailService : ILogTailService
{
    private readonly ILogSource _source;
    private readonly InMemoryLineIndex _lineIndex;
    private readonly ChunkedLineIndexBuilder _lineIndexBuilder;
    private readonly int _identityFingerprintByteCount;
    private FileSourceIdentity? _indexedIdentity;
    private DateTime? _indexedLastWriteTimeUtc;

    public LogTailService(
        ILogSource source,
        InMemoryLineIndex lineIndex,
        ChunkedLineIndexBuilder lineIndexBuilder,
        long indexedLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(indexedLength);

        _source = source ?? throw new ArgumentNullException(nameof(source));
        _lineIndex = lineIndex ?? throw new ArgumentNullException(nameof(lineIndex));
        _lineIndexBuilder = lineIndexBuilder ?? throw new ArgumentNullException(nameof(lineIndexBuilder));
        IndexedLength = indexedLength;
        _identityFingerprintByteCount = checked((int)Math.Min(indexedLength, 4096));
        _indexedIdentity = TryGetFileIdentity(source, _identityFingerprintByteCount);
        _indexedLastWriteTimeUtc = TryGetLastWriteTimeUtc(source);
    }

    public long IndexedLength { get; private set; }

    public async ValueTask<TailUpdateResult> PollAsync(CancellationToken cancellationToken = default)
    {
        var currentLength = await _source.GetLengthAsync(cancellationToken);
        var previousLength = IndexedLength;
        var currentIdentity = TryGetFileIdentity(_source, _identityFingerprintByteCount);
        var currentLastWriteTimeUtc = TryGetLastWriteTimeUtc(_source);
        var identityChanged = currentIdentity is not null
            && _indexedIdentity is not null
            && currentIdentity != _indexedIdentity;
        var sameLengthRewrite = currentLength == IndexedLength
            && currentLastWriteTimeUtc is not null
            && _indexedLastWriteTimeUtc is not null
            && currentLastWriteTimeUtc != _indexedLastWriteTimeUtc;

        if (identityChanged || currentLength < IndexedLength || sameLengthRewrite)
        {
            var rebuiltIndex = await _lineIndexBuilder.BuildAsync(_source, cancellationToken);
            _lineIndex.ReplaceAll(ReadAllLines(rebuiltIndex));
            IndexedLength = currentLength;
            _indexedIdentity = currentIdentity;
            _indexedLastWriteTimeUtc = currentLastWriteTimeUtc;
            var reason = currentLength < previousLength
                ? "truncation"
                : sameLengthRewrite
                    ? "rewrite"
                    : "rotation";
            return new TailUpdateResult(
                previousLength,
                currentLength,
                checked((int)Math.Min(int.MaxValue, _lineIndex.LineCount)),
                Truncated: currentLength < previousLength,
                WasReset: true,
                ResetReason: reason);
        }

        if (currentLength == IndexedLength)
        {
            return new TailUpdateResult(previousLength, currentLength, AddedLineCount: 0, Truncated: false);
        }

        var newLines = await _lineIndexBuilder.BuildAppendAsync(
            _source,
            IndexedLength,
            _lineIndex.LineCount,
            cancellationToken);

        _lineIndex.AddRange(newLines);
        IndexedLength = currentLength;
        _indexedIdentity = currentIdentity ?? _indexedIdentity;
        _indexedLastWriteTimeUtc = currentLastWriteTimeUtc;
        return new TailUpdateResult(previousLength, currentLength, newLines.Count, Truncated: false);
    }

    private static IReadOnlyList<LogLineReference> ReadAllLines(ILineIndex lineIndex)
    {
        var lines = new List<LogLineReference>();
        for (long i = 0; i < lineIndex.LineCount; i++)
        {
            if (lineIndex.TryGetLine(i, out var line))
            {
                lines.Add(line);
            }
        }

        return lines;
    }

    private static FileSourceIdentity? TryGetFileIdentity(ILogSource source, int fingerprintByteCount)
    {
        return source is FileLogSource fileLogSource
            ? fileLogSource.GetIdentity(fingerprintByteCount)
            : null;
    }

    private static DateTime? TryGetLastWriteTimeUtc(ILogSource source)
    {
        return source is FileLogSource fileLogSource
            ? fileLogSource.GetLastWriteTimeUtc()
            : null;
    }
}
