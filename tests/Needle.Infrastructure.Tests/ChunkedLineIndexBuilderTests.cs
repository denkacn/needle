namespace Needle.Infrastructure.Tests;

using System.Text;
using Needle.Core.Documents;
using Needle.Infrastructure.Documents;
using Needle.Infrastructure.Indexing;
using Needle.Infrastructure.Sources;

public sealed class ChunkedLineIndexBuilderTests
{
    [Fact]
    public async Task BuildsIndexForLfCrlfAndFinalLineWithoutTerminator()
    {
        var path = await WriteTempLogAsync("alpha\nbeta\r\ngamma");

        try
        {
            var source = new FileLogSource(path);
            var builder = new ChunkedLineIndexBuilder(chunkSize: 4);

            var index = await builder.BuildAsync(source);

            Assert.Equal(3, index.LineCount);
            Assert.True(index.TryGetLine(0, out var first));
            Assert.True(index.TryGetLine(1, out var second));
            Assert.True(index.TryGetLine(2, out var third));
            Assert.Equal((0, 5, 0), (first.Offset, first.Length, first.LineNumber));
            Assert.Equal((6, 4, 1), (second.Offset, second.Length, second.LineNumber));
            Assert.Equal((12, 5, 2), (third.Offset, third.Length, third.LineNumber));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task HandlesCrLfSplitAcrossChunks()
    {
        var path = await WriteTempLogAsync("abc\r\ndef");

        try
        {
            var source = new FileLogSource(path);
            var builder = new ChunkedLineIndexBuilder(chunkSize: 4);

            var index = await builder.BuildAsync(source);

            Assert.Equal(2, index.LineCount);
            Assert.True(index.TryGetLine(0, out var first));
            Assert.True(index.TryGetLine(1, out var second));
            Assert.Equal((0, 3, 0), (first.Offset, first.Length, first.LineNumber));
            Assert.Equal((5, 3, 1), (second.Offset, second.Length, second.LineNumber));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReaderReturnsRequestedLineRangeOnly()
    {
        var path = await WriteTempLogAsync("one\ntwo\nthree\nfour\n");

        try
        {
            var source = new FileLogSource(path);
            var document = new LogDocument("test", source.DisplayName, source);
            var index = await new ChunkedLineIndexBuilder(chunkSize: 5).BuildAsync(source);
            var reader = new IndexedLogDocumentReader(index);

            var lines = await reader.GetLinesAsync(document, startLine: 1, count: 2);

            Assert.Collection(
                lines,
                line => Assert.Equal("two", line.Text),
                line => Assert.Equal("three", line.Text));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static async Task<string> WriteTempLogAsync(string content)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes(content));
        return path;
    }
}
