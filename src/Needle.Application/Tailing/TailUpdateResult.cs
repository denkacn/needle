namespace Needle.Application.Tailing;

public sealed record TailUpdateResult(
    long PreviousLength,
    long CurrentLength,
    int AddedLineCount,
    bool Truncated,
    bool WasReset = false,
    string? ResetReason = null)
{
    public bool HasNewLines => AddedLineCount > 0;
}
