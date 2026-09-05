namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using Needle.Application.Documents;
using Needle.Core.Search;

internal sealed class LogSearchCoordinator
{
    private const int SearchResultLimit = 10_000;

    private readonly CancellationSlot _cancellation = new();

    public CancellationToken ResetCancellation()
    {
        return _cancellation.Reset().Token;
    }

    public void Cancel()
    {
        _cancellation.CancelAndDispose();
    }

    public bool CanSearch(bool isDocumentOpen, string searchText)
    {
        return isDocumentOpen && !string.IsNullOrWhiteSpace(searchText);
    }

    public string GetPendingStatus(string searchText)
    {
        return string.IsNullOrWhiteSpace(searchText) ? string.Empty : "Press Next to search";
    }

    public LogSearchMatch? GetNextMatch(IReadOnlyList<LogSearchMatch> matches, long currentLine)
    {
        if (matches.Count == 0)
        {
            return null;
        }

        return matches.FirstOrDefault(match => match.LineNumber > currentLine) ?? matches[0];
    }

    public LogSearchMatch? GetPreviousMatch(IReadOnlyList<LogSearchMatch> matches, long currentLine)
    {
        if (matches.Count == 0)
        {
            return null;
        }

        return matches.LastOrDefault(match => match.LineNumber < currentLine) ?? matches[^1];
    }

    public string FormatMatchStatus(IReadOnlyList<LogSearchMatch> matches, LogSearchMatch match)
    {
        var index = 0;
        for (; index < matches.Count; index++)
        {
            if (matches[index].Equals(match))
            {
                break;
            }
        }

        return $"Match {index + 1:N0}/{matches.Count:N0}";
    }

    public async Task<string> LoadMatchesAsync(
        LogDocumentSession session,
        string searchText,
        bool isCaseSensitive,
        bool isRegex,
        ObservableCollection<LogSearchMatch> target,
        CancellationToken cancellationToken)
    {
        target.Clear();

        var query = new LogSearchQuery(searchText, isCaseSensitive, isRegex);
        await foreach (var match in session.SearchService.SearchAsync(session.Document, query, cancellationToken))
        {
            target.Add(match);
            if (target.Count >= SearchResultLimit)
            {
                return $"First {SearchResultLimit:N0} matches";
            }
        }

        return target.Count == 0 ? "No matches" : $"{target.Count:N0} matches";
    }
}
