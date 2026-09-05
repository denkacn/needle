namespace Needle.Application.Filtering;

using Needle.Core.Documents;
using Needle.Core.Filtering;

public interface ILogFilterService
{
    IAsyncEnumerable<long> GetMatchingLineNumbersAsync(
        LogDocument document,
        LogFilter filter,
        CancellationToken cancellationToken = default);
}
