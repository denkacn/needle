namespace Needle.Core.Indexing;

using Needle.Core.Lines;

public sealed class InMemoryLineIndex : ILineIndex
{
    private readonly List<LogLineReference> _lines;

    public InMemoryLineIndex(IEnumerable<LogLineReference> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        _lines = lines.ToList();
    }

    public long LineCount => _lines.Count;

    public void AddRange(IEnumerable<LogLineReference> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        _lines.AddRange(lines);
    }

    public void ReplaceAll(IEnumerable<LogLineReference> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        _lines.Clear();
        _lines.AddRange(lines);
    }

    public bool TryGetLine(long lineNumber, out LogLineReference line)
    {
        if (lineNumber < 0 || lineNumber >= _lines.Count)
        {
            line = default;
            return false;
        }

        line = _lines[(int)lineNumber];
        return true;
    }
}
