namespace Needle.Core.Lines;

public sealed record LogEntry(
    LogLineReference Reference,
    string Text,
    LogLevel? Level = null,
    DateTimeOffset? Timestamp = null,
    IReadOnlyDictionary<string, string>? Fields = null,
    string? Message = null);
