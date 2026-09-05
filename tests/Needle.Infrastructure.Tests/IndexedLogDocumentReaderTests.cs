namespace Needle.Infrastructure.Tests;

using System.Text;
using Needle.Core.Documents;
using Needle.Core.Indexing;
using Needle.Core.Lines;
using Needle.Core.Sources;
using Needle.Infrastructure.Documents;

public sealed class IndexedLogDocumentReaderTests
{
    [Fact]
    public async Task ReadsRequestedLinesWithSingleContiguousStreamRead()
    {
        var bytes = Encoding.UTF8.GetBytes("zero\none\ntwo\nthree\n");
        var source = new CountingLogSource(bytes);
        var document = new LogDocument("memory", source.DisplayName, source);
        var index = new InMemoryLineIndex(
        [
            new LogLineReference(0, 4, 0),
            new LogLineReference(5, 3, 1),
            new LogLineReference(9, 3, 2),
            new LogLineReference(13, 5, 3)
        ]);
        var reader = new IndexedLogDocumentReader(index);

        var lines = await reader.GetLinesAsync(document, startLine: 1, count: 3);

        Assert.Collection(
            lines,
            line => Assert.Equal("one", line.Text),
            line => Assert.Equal("two", line.Text),
            line => Assert.Equal("three", line.Text));
        Assert.Equal(1, source.OpenCount);
        Assert.Equal(1, source.Stream.ReadCount);
        Assert.Equal(1, source.Stream.PositionSetCount);
    }

    private sealed class CountingLogSource : ILogSource
    {
        private readonly byte[] _bytes;

        public CountingLogSource(byte[] bytes)
        {
            _bytes = bytes;
            Stream = new CountingReadStream(bytes);
        }

        public string DisplayName => "memory.log";

        public int OpenCount { get; private set; }

        public CountingReadStream Stream { get; }

        public ValueTask<long> GetLengthAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult((long)_bytes.Length);
        }

        public ValueTask<Stream> OpenReadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OpenCount++;
            Stream.Position = 0;
            Stream.ResetCounters();
            return ValueTask.FromResult<Stream>(Stream);
        }
    }

    private sealed class CountingReadStream : MemoryStream
    {
        public CountingReadStream(byte[] buffer)
            : base(buffer, writable: false)
        {
        }

        public int ReadCount { get; private set; }

        public int PositionSetCount { get; private set; }

        public override long Position
        {
            get => base.Position;
            set
            {
                PositionSetCount++;
                base.Position = value;
            }
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReadCount++;
            return base.ReadAsync(buffer, cancellationToken);
        }

        public void ResetCounters()
        {
            ReadCount = 0;
            PositionSetCount = 0;
        }
    }
}
