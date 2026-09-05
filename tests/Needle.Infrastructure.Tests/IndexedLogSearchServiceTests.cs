namespace Needle.Infrastructure.Tests;

using System.Text;
using Needle.Core.Documents;
using Needle.Core.Search;
using Needle.Infrastructure.Documents;
using Needle.Infrastructure.Encoding;
using Needle.Infrastructure.Indexing;
using Needle.Infrastructure.Search;
using Needle.Infrastructure.Sources;

public sealed class IndexedLogSearchServiceTests
{
    [Fact]
    public async Task FindsPlainTextCaseInsensitive()
    {
        var matches = await SearchAsync("INFO started\nerror failed\nERROR failed again\n", new LogSearchQuery("error"));

        Assert.Equal([1, 2], matches.Select(match => match.LineNumber));
    }

    [Fact]
    public async Task FindsPlainTextCaseSensitive()
    {
        var matches = await SearchAsync(
            "INFO started\nerror failed\nERROR failed again\n",
            new LogSearchQuery("ERROR", CaseSensitive: true));

        var match = Assert.Single(matches);
        Assert.Equal(2, match.LineNumber);
    }

    [Fact]
    public async Task FindsRegexMatches()
    {
        var matches = await SearchAsync(
            "HTTP 200\nHTTP 404\nHTTP 500\n",
            new LogSearchQuery(@"HTTP\s+5\d\d", IsRegex: true));

        var match = Assert.Single(matches);
        Assert.Equal(2, match.LineNumber);
        Assert.Equal(0, match.StartIndex);
        Assert.Equal(8, match.Length);
    }

    private static async Task<IReadOnlyList<LogSearchMatch>> SearchAsync(string content, LogSearchQuery query)
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
            var search = new IndexedLogSearchService(index, reader, batchSize: 2);
            var matches = new List<LogSearchMatch>();

            await foreach (var match in search.SearchAsync(document, query))
            {
                matches.Add(match);
            }

            return matches;
        }
        finally
        {
            File.Delete(path);
        }
    }
}
