using System.Globalization;
using System.Text.Json;
using Needle.Core.Lines;

namespace Needle.Parsers;

public sealed class JsonLogParser : ILogParser
{
    private static readonly string[] TimestampKeys = ["timestamp", "time", "date", "@t"];
    private static readonly string[] LevelKeys = ["level", "logLevel", "severity", "@l"];
    private static readonly string[] MessageKeys = ["message", "msg", "@m", "renderedMessage"];

    public bool TryParse(LogLineReference reference, ReadOnlySpan<char> text, out LogEntry entry)
    {
        entry = default!;
        var trimmed = text.Trim();
        if (trimmed.Length < 2 || trimmed[0] != '{' || trimmed[^1] != '}')
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(trimmed.ToString());
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                fields[property.Name] = ToDisplayValue(property.Value);
            }

            LogLevel? level = TryGetString(document.RootElement, LevelKeys, out var levelText)
                && LogLevelParser.TryParse(levelText, out var parsedLevel)
                    ? parsedLevel
                    : null;

            var timestamp = TryGetString(document.RootElement, TimestampKeys, out var timestampText)
                && DateTimeOffset.TryParse(timestampText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsedTimestamp)
                    ? parsedTimestamp
                    : (DateTimeOffset?)null;

            var message = TryGetString(document.RootElement, MessageKeys, out var messageText)
                ? messageText
                : null;

            entry = new LogEntry(reference, text.ToString(), level, timestamp, fields, message);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryGetString(JsonElement element, IEnumerable<string> keys, out string value)
    {
        foreach (var key in keys)
        {
            if (!element.TryGetProperty(key, out var property))
            {
                continue;
            }

            value = ToDisplayValue(property);
            return !string.IsNullOrWhiteSpace(value);
        }

        value = string.Empty;
        return false;
    }

    private static string ToDisplayValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            _ => element.GetRawText()
        };
    }
}
