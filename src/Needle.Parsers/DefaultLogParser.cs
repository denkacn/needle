using Needle.Core.Lines;

namespace Needle.Parsers;

public sealed class DefaultLogParser : ILogParser
{
    private readonly CompositeLogParser _inner = new(
    [
        new JsonLogParser(),
        new TextLogLevelParser(),
        new RawLogParser()
    ]);

    public bool TryParse(LogLineReference reference, ReadOnlySpan<char> text, out LogEntry entry)
    {
        return _inner.TryParse(reference, text, out entry);
    }
}
