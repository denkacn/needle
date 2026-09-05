namespace Needle.Avalonia.ViewModels;

using Needle.Application.Highlighting;
using Needle.Core.Highlighting;
using Needle.Core.Lines;
using Needle.Parsers;

internal sealed class LogLinePresenter
{
    private readonly ILogParser _logParser;
    private readonly StructuredLogFormatter _structuredFormatter;

    public LogLinePresenter(ILogParser logParser, StructuredLogFormatter structuredFormatter)
    {
        _logParser = logParser;
        _structuredFormatter = structuredFormatter;
    }

    public LogLineViewModel Create(
        LogEntry entry,
        IEnumerable<LogHighlightRule> highlightRules,
        Func<long, bool> isBookmarkedLine,
        Func<long, bool> isTriggeredLine,
        bool isStructuredMode,
        long selectedLineNumber)
    {
        var isBookmarked = isBookmarkedLine(entry.Reference.LineNumber);
        var isTriggered = isTriggeredLine(entry.Reference.LineNumber);
        var isSelected = selectedLineNumber == entry.Reference.LineNumber + 1;
        var displayEntry = isStructuredMode && _logParser.TryParse(entry.Reference, entry.Text, out var parsedEntry)
            ? parsedEntry
            : entry;
        var text = isStructuredMode ? _structuredFormatter.Format(displayEntry) : entry.Text;
        var level = displayEntry.Level;

        foreach (var rule in highlightRules)
        {
            if (!LogHighlightMatcher.IsMatch(entry.Text, rule))
            {
                continue;
            }

            return new LogLineViewModel(
                entry.Reference.LineNumber + 1,
                text,
                rule.Foreground,
                isBookmarked && rule.Background is null ? "#2B2412" : rule.Background,
                isTriggered ? "#EF4444" : isBookmarked ? "#E0A800" : AccentFor(rule.Pattern),
                isBookmarked,
                isSelected,
                isTriggered);
        }

        return new LogLineViewModel(
            entry.Reference.LineNumber + 1,
            text,
            Foreground: ForegroundFor(level),
            Background: isBookmarked ? "#2B2412" : null,
            Accent: isTriggered ? "#EF4444" : isBookmarked ? "#E0A800" : AccentFor(level),
            IsBookmarked: isBookmarked,
            IsSelected: isSelected,
            IsTriggered: isTriggered);
    }

    private static string? AccentFor(string pattern)
    {
        return pattern.ToUpperInvariant() switch
        {
            "FATAL" or "CRITICAL" or "ERROR" => "#E05252",
            "WARN" or "WARNING" => "#D7A63F",
            "INFO" => "#1497D4",
            "DEBUG" => "#777F88",
            "TRACE" => "#5D6670",
            _ => null
        };
    }

    private static string? ForegroundFor(LogLevel? level)
    {
        return level switch
        {
            LogLevel.Fatal or LogLevel.Critical or LogLevel.Error => "#FFD6D1",
            LogLevel.Warning => "#FFE1A3",
            LogLevel.Information or LogLevel.Debug or LogLevel.Trace => null,
            _ => null
        };
    }

    private static string? AccentFor(LogLevel? level)
    {
        return level switch
        {
            LogLevel.Fatal or LogLevel.Critical or LogLevel.Error => "#E05252",
            LogLevel.Warning => "#D7A63F",
            LogLevel.Information => "#1497D4",
            LogLevel.Debug => "#777F88",
            LogLevel.Trace => "#5D6670",
            _ => null
        };
    }
}
