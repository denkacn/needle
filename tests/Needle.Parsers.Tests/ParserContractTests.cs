namespace Needle.Parsers.Tests;

using Needle.Core.Lines;

public sealed class ParserContractTests
{
    [Fact]
    public void ParserAssemblyIsLoadable()
    {
        Assert.NotNull(typeof(ILogParser).Assembly);
    }

    [Fact]
    public void JsonLogParserParsesCommonStructuredFields()
    {
        var parser = new JsonLogParser();
        var reference = new LogLineReference(10, 120, 3);

        var parsed = parser.TryParse(
            reference,
            """{"timestamp":"2026-08-11T12:13:14Z","level":"Error","message":"Database timeout","requestId":42}""",
            out var entry);

        Assert.True(parsed);
        Assert.Equal(reference, entry.Reference);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal("Database timeout", entry.Message);
        Assert.Equal(2026, entry.Timestamp?.Year);
        Assert.Equal("42", entry.Fields?["requestId"]);
    }

    [Fact]
    public void JsonLogParserReturnsFalseForInvalidJson()
    {
        var parser = new JsonLogParser();

        var parsed = parser.TryParse(new LogLineReference(0, 3, 0), "{bad", out _);

        Assert.False(parsed);
    }

    [Theory]
    [InlineData("2026-08-11 12:00:00 INFO Server started", LogLevel.Information)]
    [InlineData("[WARN] response is slow", LogLevel.Warning)]
    [InlineData("fatal: process crashed", LogLevel.Fatal)]
    public void TextLogLevelParserDetectsKnownLevels(string text, LogLevel expectedLevel)
    {
        var parser = new TextLogLevelParser();

        var parsed = parser.TryParse(new LogLineReference(0, text.Length, 0), text, out var entry);

        Assert.True(parsed);
        Assert.Equal(expectedLevel, entry.Level);
    }

    [Fact]
    public void DefaultLogParserFallsBackToRawEntry()
    {
        var parser = new DefaultLogParser();

        var parsed = parser.TryParse(new LogLineReference(0, 11, 0), "hello world", out var entry);

        Assert.True(parsed);
        Assert.Equal("hello world", entry.Text);
        Assert.Null(entry.Level);
        Assert.Null(entry.Fields);
    }

    [Fact]
    public void CompositeLogParserUsesFirstParserThatMatches()
    {
        var parser = new CompositeLogParser([new TextLogLevelParser(), new RawLogParser()]);

        var parsed = parser.TryParse(new LogLineReference(0, 12, 0), "INFO startup", out var entry);

        Assert.True(parsed);
        Assert.Equal(LogLevel.Information, entry.Level);
    }

    [Fact]
    public void StructuredLogFormatterFormatsParsedJsonAsStableColumns()
    {
        var parser = new JsonLogParser();
        var formatter = new StructuredLogFormatter();
        parser.TryParse(
            new LogLineReference(0, 120, 0),
            """{"@t":"2026-08-11T12:13:14Z","@l":"Warning","@m":"Slow request","elapsed":152,"path":"/api/items"}""",
            out var entry);

        var formatted = formatter.Format(entry);

        Assert.Contains("2026-08-11 12:13:14.000", formatted);
        Assert.Contains("WARN", formatted);
        Assert.Contains("Slow request", formatted);
        Assert.Contains("elapsed=152", formatted);
        Assert.Contains("path=/api/items", formatted);
    }

    [Fact]
    public void StructuredLogFormatterKeepsRawTextWhenNothingWasParsed()
    {
        var formatter = new StructuredLogFormatter();
        var entry = new LogEntry(new LogLineReference(0, 9, 0), "raw line");

        var formatted = formatter.Format(entry);

        Assert.Equal("raw line", formatted);
    }
}
