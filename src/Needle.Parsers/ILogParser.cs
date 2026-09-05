using Needle.Core.Lines;

namespace Needle.Parsers;

public interface ILogParser
{
    bool TryParse(LogLineReference reference, ReadOnlySpan<char> text, out LogEntry entry);
}
