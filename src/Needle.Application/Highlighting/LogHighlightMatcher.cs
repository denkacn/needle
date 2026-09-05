namespace Needle.Application.Highlighting;

using System.Text.RegularExpressions;
using Needle.Core.Highlighting;

public static class LogHighlightMatcher
{
    public static bool IsMatch(string text, LogHighlightRule rule)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(rule);

        if (!rule.Enabled || string.IsNullOrWhiteSpace(rule.Pattern))
        {
            return false;
        }

        if (!rule.IsRegex)
        {
            return text.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase);
        }

        return Regex.IsMatch(
            text,
            rule.Pattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(2));
    }
}
