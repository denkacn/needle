using System.Globalization;
using System.Text;
using Needle.Core.Lines;

namespace Needle.Parsers;

public sealed class StructuredLogFormatter
{
    private static readonly HashSet<string> CommonFieldKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "timestamp",
        "time",
        "date",
        "@t",
        "level",
        "logLevel",
        "severity",
        "@l",
        "message",
        "msg",
        "@m",
        "renderedMessage"
    };

    public string Format(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Timestamp is null && entry.Level is null && entry.Message is null && entry.Fields is null)
        {
            return entry.Text;
        }

        var builder = new StringBuilder(entry.Text.Length);
        AppendTimestamp(builder, entry.Timestamp);
        AppendLevel(builder, entry.Level);
        AppendPart(builder, entry.Message ?? entry.Text);
        AppendFields(builder, entry.Fields);

        return builder.ToString();
    }

    private static void AppendTimestamp(StringBuilder builder, DateTimeOffset? timestamp)
    {
        if (timestamp is null)
        {
            return;
        }

        AppendPart(builder, timestamp.Value.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture));
    }

    private static void AppendLevel(StringBuilder builder, LogLevel? level)
    {
        if (level is null)
        {
            return;
        }

        AppendPart(builder, ToDisplayLevel(level.Value));
    }

    private static void AppendFields(StringBuilder builder, IReadOnlyDictionary<string, string>? fields)
    {
        if (fields is null || fields.Count == 0)
        {
            return;
        }

        foreach (var field in fields.OrderBy(field => field.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (CommonFieldKeys.Contains(field.Key) || string.IsNullOrEmpty(field.Value))
            {
                continue;
            }

            AppendPart(builder, $"{field.Key}={field.Value}");
        }
    }

    private static void AppendPart(StringBuilder builder, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(" | ");
        }

        builder.Append(value);
    }

    private static string ToDisplayLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Fatal => "FATAL",
            LogLevel.Critical => "CRITICAL",
            _ => level.ToString().ToUpperInvariant()
        };
    }
}
