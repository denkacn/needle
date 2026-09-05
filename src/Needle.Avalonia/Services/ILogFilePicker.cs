namespace Needle.Avalonia.Services;

public interface ILogFilePicker
{
    Task<string?> PickLogFileAsync(CancellationToken cancellationToken = default);
}
