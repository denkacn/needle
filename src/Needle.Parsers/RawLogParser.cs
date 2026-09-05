using Needle.Core.Lines;

namespace Needle.Parsers;

public sealed class RawLogParser : ILogParser
{
    public bool TryParse(LogLineReference reference, ReadOnlySpan<char> text, out LogEntry entry)
    {
        entry = new LogEntry(reference, text.ToString());
        return true;
    }
}
