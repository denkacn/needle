namespace Needle.Application.Search;

using Needle.Core.Documents;
using Needle.Core.Search;

public interface ILogSearchService
{
    IAsyncEnumerable<LogSearchMatch> SearchAsync(
        LogDocument document,
        LogSearchQuery query,
        CancellationToken cancellationToken = default);
}
