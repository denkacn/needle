namespace Needle.Application.Filtering;

using Needle.Core.Filtering;

public static class LogFilterParser
{
    public static LogFilter Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new LogFilter([], []);
        }

        var include = new List<string>();
        var exclude = new List<string>();
        var parts = text.Split([';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            if (part.Length == 0)
            {
                continue;
            }

            if (part[0] == '-')
            {
                var pattern = part[1..].Trim();
                if (pattern.Length > 0)
                {
                    exclude.Add(pattern);
                }

                continue;
            }

            if (part[0] == '+')
            {
                var pattern = part[1..].Trim();
                if (pattern.Length > 0)
                {
                    include.Add(pattern);
                }

                continue;
            }

            include.Add(part);
        }

        return new LogFilter(include, exclude);
    }
}
