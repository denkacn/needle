namespace Needle.Avalonia.ViewModels;

using Needle.Application.Documents;
using Needle.Application.Filtering;
using Needle.Core.Filtering;

internal sealed class LogFilterCoordinator
{
    public bool HasFilter(string filterText)
    {
        var filter = LogFilterParser.Parse(filterText);
        return filter.IncludePatterns.Count > 0 || filter.ExcludePatterns.Count > 0;
    }

    public bool HasVisibilityFilter(string filterText, IReadOnlyList<string> excludePatterns)
    {
        return HasFilter(filterText) || excludePatterns.Count > 0;
    }

    public async IAsyncEnumerable<long> GetMatchingLineNumbersAsync(
        LogDocumentSession session,
        string filterText,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var lineNumber in GetVisibleLineNumbersAsync(session, filterText, [], cancellationToken))
        {
            yield return lineNumber;
        }
    }

    public async IAsyncEnumerable<long> GetVisibleLineNumbersAsync(
        LogDocumentSession session,
        string filterText,
        IReadOnlyList<string> excludePatterns,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var filter = LogFilterParser.Parse(filterText);
        if (excludePatterns.Count > 0)
        {
            filter = new LogFilter(
                filter.IncludePatterns,
                [.. filter.ExcludePatterns.Concat(excludePatterns)]);
        }

        await foreach (var lineNumber in session.FilterService.GetMatchingLineNumbersAsync(
                           session.Document,
                           filter,
                           cancellationToken))
        {
            yield return lineNumber;
        }
    }
}
