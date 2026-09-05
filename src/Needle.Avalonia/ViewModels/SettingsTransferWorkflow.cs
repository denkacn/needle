namespace Needle.Avalonia.ViewModels;

using System.Text.Json;
using Needle.Application.Preferences;

internal sealed class SettingsTransferWorkflow
{
    private readonly Func<UserPreferences> _createPreferences;
    private readonly Action<UserPreferences> _applyPreferences;
    private readonly Func<Task> _savePreferencesAsync;
    private readonly Func<bool> _hasOpenDocument;
    private readonly Func<Task> _reloadViewportAsync;
    private readonly Action<string> _setStatus;

    public SettingsTransferWorkflow(
        Func<UserPreferences> createPreferences,
        Action<UserPreferences> applyPreferences,
        Func<Task> savePreferencesAsync,
        Func<bool> hasOpenDocument,
        Func<Task> reloadViewportAsync,
        Action<string> setStatus)
    {
        _createPreferences = createPreferences;
        _applyPreferences = applyPreferences;
        _savePreferencesAsync = savePreferencesAsync;
        _hasOpenDocument = hasOpenDocument;
        _reloadViewportAsync = reloadViewportAsync;
        _setStatus = setStatus;
    }

    public async Task ExportAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        await _savePreferencesAsync();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, _createPreferences(), new JsonSerializerOptions { WriteIndented = true });
        _setStatus($"Settings exported to {Path.GetFileName(path)}");
    }

    public async Task ImportAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            await using var stream = File.OpenRead(path);
            var preferences = await JsonSerializer.DeserializeAsync<UserPreferences>(stream);
            if (preferences is null)
            {
                _setStatus("Settings import failed");
                return;
            }

            _applyPreferences(preferences);
            await _savePreferencesAsync();
            if (_hasOpenDocument())
            {
                await _reloadViewportAsync();
            }

            _setStatus($"Settings imported from {Path.GetFileName(path)}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            _setStatus($"Settings import failed: {ex.Message}");
        }
    }
}
