namespace Needle.Avalonia.Services;

public sealed class WindowLogFilePicker : ILogFilePicker
{
    private readonly global::Avalonia.Controls.Window _window;

    public WindowLogFilePicker(global::Avalonia.Controls.Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    public async Task<string?> PickLogFileAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var files = await _window.StorageProvider.OpenFilePickerAsync(new global::Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Open file in Needle",
            AllowMultiple = false,
            FileTypeFilter =
            [
                global::Avalonia.Platform.Storage.FilePickerFileTypes.All,
                new global::Avalonia.Platform.Storage.FilePickerFileType("Log files")
                {
                    Patterns = ["*.log", "*.txt", "*.out", "*.err"],
                    MimeTypes = ["text/plain", "application/octet-stream"]
                }
            ]
        });

        cancellationToken.ThrowIfCancellationRequested();
        return files.Count == 0 ? null : files[0].Path.LocalPath;
    }
}
