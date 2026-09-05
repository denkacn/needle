namespace Needle.Core.Search;

public sealed record LogSearchQuery(
    string Pattern,
    bool CaseSensitive = false,
    bool IsRegex = false);
