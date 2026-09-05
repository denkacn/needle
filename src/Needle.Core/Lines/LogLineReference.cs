namespace Needle.Core.Lines;

public readonly record struct LogLineReference(
    long Offset,
    int Length,
    long LineNumber);
