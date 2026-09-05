namespace Needle.Core.Indexing;

using Needle.Core.Sources;

public interface ILineIndexBuilder
{
    ValueTask<ILineIndex> BuildAsync(
        ILogSource source,
        CancellationToken cancellationToken = default);
}
