namespace Needle.Core.Bookmarks;

public sealed class LogBookmarkSet
{
    private readonly SortedSet<long> _lineNumbers = [];

    public int Count => _lineNumbers.Count;

    public IReadOnlyCollection<long> LineNumbers => _lineNumbers;

    public bool Contains(long lineNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(lineNumber);
        return _lineNumbers.Contains(lineNumber);
    }

    public bool Toggle(long lineNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(lineNumber);

        if (_lineNumbers.Remove(lineNumber))
        {
            return false;
        }

        _lineNumbers.Add(lineNumber);
        return true;
    }

    public void Clear()
    {
        _lineNumbers.Clear();
    }

    public bool TryGetNext(long currentLineNumber, out long lineNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(currentLineNumber);

        foreach (var candidate in _lineNumbers)
        {
            if (candidate > currentLineNumber)
            {
                lineNumber = candidate;
                return true;
            }
        }

        if (_lineNumbers.Count > 0)
        {
            lineNumber = _lineNumbers.Min;
            return true;
        }

        lineNumber = default;
        return false;
    }

    public bool TryGetPrevious(long currentLineNumber, out long lineNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(currentLineNumber);

        foreach (var candidate in _lineNumbers.Reverse())
        {
            if (candidate < currentLineNumber)
            {
                lineNumber = candidate;
                return true;
            }
        }

        if (_lineNumbers.Count > 0)
        {
            lineNumber = _lineNumbers.Max;
            return true;
        }

        lineNumber = default;
        return false;
    }
}
