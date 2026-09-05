namespace Needle.Core.Indexing;

using Needle.Core.Lines;

public interface ILineIndex
{
    long LineCount { get; }

    bool TryGetLine(long lineNumber, out LogLineReference line);
}
