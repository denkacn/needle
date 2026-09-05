using Needle.Core.Lines;

namespace Needle.Parsers;

public sealed class CompositeLogParser : ILogParser
{
    private readonly IReadOnlyList<ILogParser> _parsers;

    public CompositeLogParser(IEnumerable<ILogParser> parsers)
    {
        ArgumentNullException.ThrowIfNull(parsers);
        _parsers = parsers.ToArray();
    }

    public bool TryParse(LogLineReference reference, ReadOnlySpan<char> text, out LogEntry entry)
    {
        foreach (var parser in _parsers)
        {
            if (parser.TryParse(reference, text, out entry))
            {
                return true;
            }
        }

        entry = default!;
        return false;
    }
}
