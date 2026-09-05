namespace Needle.Infrastructure.Tests;

using System.Text;
using Needle.Core.Documents;
using Needle.Infrastructure.Documents;
using Needle.Infrastructure.Encoding;
using Needle.Infrastructure.Indexing;
using Needle.Infrastructure.Sources;

public sealed class EncodedLogReaderTests
{
    [Fact]
    public async Task ReadsUtf8BomWithoutReturningBomCharacter()
    {
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("alpha\nbeta")).ToArray();

        var lines = await ReadLinesAsync(bytes);

        Assert.Collection(
            lines,
            line => Assert.Equal("alpha", line.Text),
            line => Assert.Equal("beta", line.Text));
    }

    [Fact]
    public async Task ReadsUtf16LittleEndianLines()
    {
        var bytes = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("alpha\r\nbeta")).ToArray();

        var lines = await ReadLinesAsync(bytes);

        Assert.Collection(
            lines,
            line => Assert.Equal("alpha", line.Text),
            line => Assert.Equal("beta", line.Text));
    }

    [Fact]
    public async Task ReadsUtf16BigEndianLines()
    {
        var bytes = Encoding.BigEndianUnicode.GetPreamble()
            .Concat(Encoding.BigEndianUnicode.GetBytes("alpha\nbeta"))
            .ToArray();

        var lines = await ReadLinesAsync(bytes);

        Assert.Collection(
            lines,
            line => Assert.Equal("alpha", line.Text),
            line => Assert.Equal("beta", line.Text));
    }

    [Fact]
    public async Task ReadsWindows1251CyrillicLines()
    {
        var bytes = new byte[]
        {
            0xEB, 0xEE, 0xE3, 0x3A, 0x20, 0xCF, 0xF0, 0xE8, 0xE2, 0xE5, 0xF2, 0x0A,
            0xEE, 0xF8, 0xE8, 0xE1, 0xEA, 0xE0
        };

        var lines = await ReadLinesAsync(bytes);

        Assert.Collection(
            lines,
            line => Assert.Equal("лог: Привет", line.Text),
            line => Assert.Equal("ошибка", line.Text));
    }

    private static async Task<IReadOnlyList<Core.Lines.LogEntry>> ReadLinesAsync(byte[] bytes)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        await File.WriteAllBytesAsync(path, bytes);

        try
        {
            var source = new FileLogSource(path);
            var document = new LogDocument("test", source.DisplayName, source);
            var detection = await new EncodingDetector().DetectAsync(source);
            var index = await new ChunkedLineIndexBuilder(chunkSize: 4, encodingDetection: detection).BuildAsync(source);
            var reader = new IndexedLogDocumentReader(index, detection.Encoding);

            return await reader.GetLinesAsync(document, 0, 10);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
