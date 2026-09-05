namespace Needle.Infrastructure.Indexing;

using System.Buffers;
using System.Text;
using Needle.Core.Indexing;
using Needle.Core.Lines;
using Needle.Core.Sources;
using Needle.Infrastructure.Encoding;

public sealed class ChunkedLineIndexBuilder : ILineIndexBuilder
{
    public const int DefaultChunkSize = 128 * 1024;

    private readonly int _chunkSize;
    private readonly EncodingDetectionResult? _encodingDetection;

    public ChunkedLineIndexBuilder(
        int chunkSize = DefaultChunkSize,
        EncodingDetectionResult? encodingDetection = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);
        _chunkSize = chunkSize;
        _encodingDetection = encodingDetection;
    }

    public async ValueTask<ILineIndex> BuildAsync(
        ILogSource source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (_encodingDetection?.Encoding.CodePage is 1200 or 1201)
        {
            return await BuildUtf16Async(source, cancellationToken);
        }

        return await BuildSingleByteLineIndexAsync(source, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LogLineReference>> BuildAppendAsync(
        ILogSource source,
        long startOffset,
        long firstLineNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(startOffset);
        ArgumentOutOfRangeException.ThrowIfNegative(firstLineNumber);

        if (_encodingDetection?.Encoding.CodePage is 1200 or 1201)
        {
            return await BuildUtf16LinesAsync(source, startOffset, firstLineNumber, cancellationToken);
        }

        return await BuildSingleByteLinesAsync(source, startOffset, firstLineNumber, cancellationToken);
    }

    private async ValueTask<ILineIndex> BuildSingleByteLineIndexAsync(
        ILogSource source,
        CancellationToken cancellationToken)
    {
        var lines = await BuildSingleByteLinesAsync(
            source,
            _encodingDetection?.PreambleLength ?? 0,
            firstLineNumber: 0,
            cancellationToken);

        return new InMemoryLineIndex(lines);
    }

    private async ValueTask<IReadOnlyList<LogLineReference>> BuildSingleByteLinesAsync(
        ILogSource source,
        long startOffset,
        long firstLineNumber,
        CancellationToken cancellationToken)
    {
        await using var stream = await source.OpenReadAsync(cancellationToken);
        stream.Position = startOffset;
        var lines = new List<LogLineReference>();
        var buffer = ArrayPool<byte>.Shared.Rent(_chunkSize);

        try
        {
            long absoluteOffset = startOffset;
            long lineStartOffset = startOffset;
            long lineNumber = firstLineNumber;
            var previousWasCarriageReturn = false;

            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, _chunkSize), cancellationToken);
                if (bytesRead == 0)
                {
                    break;
                }

                for (var i = 0; i < bytesRead; i++)
                {
                    var value = buffer[i];
                    var currentOffset = absoluteOffset + i;

                    if (value == (byte)'\r')
                    {
                        AddLine(lines, lineStartOffset, currentOffset, lineNumber++);
                        lineStartOffset = currentOffset + 1;
                        previousWasCarriageReturn = true;
                        continue;
                    }

                    if (value == (byte)'\n')
                    {
                        if (!previousWasCarriageReturn)
                        {
                            AddLine(lines, lineStartOffset, currentOffset, lineNumber++);
                        }

                        lineStartOffset = currentOffset + 1;
                        previousWasCarriageReturn = false;
                        continue;
                    }

                    previousWasCarriageReturn = false;
                }

                absoluteOffset += bytesRead;
            }

            if (lineStartOffset < absoluteOffset)
            {
                AddLine(lines, lineStartOffset, absoluteOffset, lineNumber);
            }

            return lines;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async ValueTask<ILineIndex> BuildUtf16Async(
        ILogSource source,
        CancellationToken cancellationToken)
    {
        var lines = await BuildUtf16LinesAsync(
            source,
            _encodingDetection?.PreambleLength ?? 0,
            firstLineNumber: 0,
            cancellationToken);

        return new InMemoryLineIndex(lines);
    }

    private async ValueTask<IReadOnlyList<LogLineReference>> BuildUtf16LinesAsync(
        ILogSource source,
        long startOffset,
        long firstLineNumber,
        CancellationToken cancellationToken)
    {
        await using var stream = await source.OpenReadAsync(cancellationToken);
        stream.Position = startOffset;
        var lines = new List<LogLineReference>();
        var buffer = ArrayPool<byte>.Shared.Rent(_chunkSize);

        try
        {
            var bigEndian = _encodingDetection?.Encoding.CodePage == 1201;
            long absoluteOffset = startOffset;
            long lineStartOffset = startOffset;
            long lineNumber = firstLineNumber;
            var previousWasCarriageReturn = false;
            byte? pendingByte = null;
            long pendingOffset = 0;

            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, _chunkSize), cancellationToken);
                if (bytesRead == 0)
                {
                    break;
                }

                for (var i = 0; i < bytesRead; i++)
                {
                    var currentOffset = absoluteOffset + i;
                    if (pendingByte is null)
                    {
                        pendingByte = buffer[i];
                        pendingOffset = currentOffset;
                        continue;
                    }

                    var first = pendingByte.Value;
                    var second = buffer[i];
                    pendingByte = null;

                    var value = bigEndian
                        ? (ushort)((first << 8) | second)
                        : (ushort)(first | (second << 8));

                    if (value == '\r')
                    {
                        AddLine(lines, lineStartOffset, pendingOffset, lineNumber++);
                        lineStartOffset = pendingOffset + 2;
                        previousWasCarriageReturn = true;
                        continue;
                    }

                    if (value == '\n')
                    {
                        if (!previousWasCarriageReturn)
                        {
                            AddLine(lines, lineStartOffset, pendingOffset, lineNumber++);
                        }

                        lineStartOffset = pendingOffset + 2;
                        previousWasCarriageReturn = false;
                        continue;
                    }

                    previousWasCarriageReturn = false;
                }

                absoluteOffset += bytesRead;
            }

            if (lineStartOffset < absoluteOffset)
            {
                AddLine(lines, lineStartOffset, absoluteOffset, lineNumber);
            }

            return lines;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static void AddLine(
        List<LogLineReference> lines,
        long startOffset,
        long endOffset,
        long lineNumber)
    {
        var length = checked((int)(endOffset - startOffset));
        lines.Add(new LogLineReference(startOffset, length, lineNumber));
    }
}
