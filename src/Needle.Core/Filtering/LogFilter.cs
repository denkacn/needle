namespace Needle.Core.Filtering;

public sealed record LogFilter(
    IReadOnlyList<string> IncludePatterns,
    IReadOnlyList<string> ExcludePatterns);
