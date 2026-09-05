using Needle.Core.Lines;

namespace Needle.Parsers;

public sealed class TextLogLevelParser : ILogParser
{
    public bool TryParse(LogLineReference reference, ReadOnlySpan<char> text, out LogEntry entry)
    {
        if (TryFindLevel(text, out var level))
        {
            entry = new LogEntry(reference, text.ToString(), level);
            return true;
        }

        entry = default!;
        return false;
    }

    private static bool TryFindLevel(ReadOnlySpan<char> text, out LogLevel level)
    {
        var tokenStart = -1;
        for (var i = 0; i <= text.Length; i++)
        {
            var atEnd = i == text.Length;
            var isTokenChar = !atEnd && (char.IsLetter(text[i]) || text[i] == '_');
            if (isTokenChar)
            {
                if (tokenStart < 0)
                {
                    tokenStart = i;
                }

                continue;
            }

            if (tokenStart >= 0)
            {
                var token = text[tokenStart..i].Trim('_');
                if (LogLevelParser.TryParse(token, out level))
                {
                    return true;
                }

                tokenStart = -1;
            }
        }

        level = default;
        return false;
    }
}
