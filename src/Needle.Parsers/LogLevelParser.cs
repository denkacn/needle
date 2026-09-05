using Needle.Core.Lines;

namespace Needle.Parsers;

public static class LogLevelParser
{
    public static bool TryParse(ReadOnlySpan<char> text, out LogLevel level)
    {
        var normalized = text.Trim();
        if (normalized.Equals("TRACE", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("TRC", StringComparison.OrdinalIgnoreCase))
        {
            level = LogLevel.Trace;
            return true;
        }

        if (normalized.Equals("DEBUG", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("DBG", StringComparison.OrdinalIgnoreCase))
        {
            level = LogLevel.Debug;
            return true;
        }

        if (normalized.Equals("INFO", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("INFORMATION", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("INF", StringComparison.OrdinalIgnoreCase))
        {
            level = LogLevel.Information;
            return true;
        }

        if (normalized.Equals("WARN", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("WARNING", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("WRN", StringComparison.OrdinalIgnoreCase))
        {
            level = LogLevel.Warning;
            return true;
        }

        if (normalized.Equals("ERROR", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("ERR", StringComparison.OrdinalIgnoreCase))
        {
            level = LogLevel.Error;
            return true;
        }

        if (normalized.Equals("FATAL", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("FTL", StringComparison.OrdinalIgnoreCase))
        {
            level = LogLevel.Fatal;
            return true;
        }

        if (normalized.Equals("CRITICAL", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("CRT", StringComparison.OrdinalIgnoreCase))
        {
            level = LogLevel.Critical;
            return true;
        }

        level = default;
        return false;
    }
}
