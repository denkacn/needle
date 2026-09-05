namespace Needle.Core.Search;

public sealed record LogSearchMatch(
    long LineNumber,
    int StartIndex,
    int Length);
