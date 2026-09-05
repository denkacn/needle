namespace Needle.Infrastructure.Tests;

using System.Text;
using Needle.Core.Documents;
using Needle.Application.Filtering;
using Needle.Infrastructure.Documents;
using Needle.Infrastructure.Encoding;
using Needle.Infrastructure.Filtering;
using Needle.Infrastructure.Indexing;
using Needle.Infrastructure.Sources;

public sealed class IndexedLogFilterServiceTests
{
    [Fact]
    public void ParserSeparatesIncludeAndExcludeRules()
    {
        var filter = LogFilterParser.Parse("+ ERROR; + Network; - Heartbeat");

        Assert.Equal(["ERROR", "Network"], filter.IncludePatterns);
        Assert.Equal(["Heartbeat"], filter.ExcludePatterns);
    }

    [Fact]
    public async Task ReturnsLinesMatchingIncludeAndExcludeRules()
    {
        var matches = await FilterAsync(
            "INFO Started\nERROR Network disconnected\nERROR Heartbeat failed\nWARN Network slow\n",
            "+ ERROR; - Heartbeat");

        var line = Assert.Single(matches);
        Assert.Equal(1, line);
    }

    [Fact]
    public async Task ExcludeOnlyFilterRemovesMatchingLines()
    {
        var matches = await FilterAsync(
            "INFO Started\nDEBUG Heartbeat\nWARN Slow\n",
            "- Heartbeat");

        Assert.Equal([0, 2], matches);
    }

    private static async Task<IReadOnlyList<long>> FilterAsync(string content, string filterText)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes(content));

        try
        {
            var source = new FileLogSource(path);
            var document = new LogDocument("test", source.DisplayName, source);
            var detection = await new EncodingDetector().DetectAsync(source);
            var index = await new ChunkedLineIndexBuilder(chunkSize: 8, encodingDetection: detection).BuildAsync(source);
            var reader = new IndexedLogDocumentReader(index, detection.Encoding);
            var service = new IndexedLogFilterService(index, reader, batchSize: 2);
            var lines = new List<long>();

            await foreach (var line in service.GetMatchingLineNumbersAsync(document, LogFilterParser.Parse(filterText)))
            {
                lines.Add(line);
            }

            return lines;
        }
        finally
        {
            File.Delete(path);
        }
    }
}
