namespace Needle.Application.Documents;

using Needle.Core.Documents;
using Needle.Core.Lines;

public interface ILogDocumentReader
{
    ValueTask<IReadOnlyList<LogEntry>> GetLinesAsync(
        LogDocument document,
        long startLine,
        int count,
        CancellationToken cancellationToken = default);
}
