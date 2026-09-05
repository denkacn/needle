namespace Needle.Avalonia.Services;

public sealed class NoOpLogFilePicker : ILogFilePicker
{
    public Task<string?> PickLogFileAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(null);
    }
}
