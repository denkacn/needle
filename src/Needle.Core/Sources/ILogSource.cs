namespace Needle.Core.Sources;

public interface ILogSource
{
    string DisplayName { get; }

    ValueTask<long> GetLengthAsync(CancellationToken cancellationToken = default);

    ValueTask<Stream> OpenReadAsync(CancellationToken cancellationToken = default);
}
