using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Needle.Avalonia.Views;

internal sealed class WindowDialogService
{
    private static readonly FilePickerFileType SettingsFileType = new("Needle settings")
    {
        Patterns = ["*.needle-settings.json", "*.json"]
    };

    private readonly Window _owner;

    public WindowDialogService(Window owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public Task<string?> PickColorAsync(string? initialColor)
    {
        var picker = new ColorPickerWindow(initialColor);
        return picker.ShowDialog<string?>(_owner);
    }

    public Task<string?> PickLogFontAsync(
        IReadOnlyList<string> systemFontNames,
        IEnumerable<string> existingFontNames)
    {
        var dialog = new AddLogFontWindow(systemFontNames, existingFontNames);
        return dialog.ShowDialog<string?>(_owner);
    }

    public async Task<string?> PickSettingsExportFileAsync()
    {
        var file = await _owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Needle settings",
            SuggestedFileName = "needle-settings.json",
            FileTypeChoices = [SettingsFileType]
        });

        return file?.Path.LocalPath;
    }

    public async Task<string?> PickSettingsImportFileAsync()
    {
        var files = await _owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import Needle settings",
            AllowMultiple = false,
            FileTypeFilter = [SettingsFileType, FilePickerFileTypes.All]
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    public Task<bool> ConfirmAsync(string title, string message, string confirmText)
    {
        var dialog = new ConfirmationWindow(title, message, confirmText);
        return dialog.ShowDialog<bool>(_owner);
    }
}

internal static class WindowDialogServiceResolver
{
    public static WindowDialogService? TryCreate(Control control)
    {
        return TopLevel.GetTopLevel(control) is Window owner
            ? new WindowDialogService(owner)
            : null;
    }
}
