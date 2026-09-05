namespace Needle.Avalonia.Tests;

using Needle.Avalonia.ViewModels;
using Needle.Application.Preferences;
using Needle.Application.Workspace;

public sealed class SettingsTransferWorkflowTests
{
    [Fact]
    public async Task ExportAsyncWritesPreferencesFile()
    {
        using var tempFile = new TempJsonFile();
        var workflow = new SettingsTransferWorkflow(
            CreatePreferences,
            _ => { },
            () => Task.CompletedTask,
            () => false,
            () => Task.CompletedTask,
            _ => { });

        await workflow.ExportAsync(tempFile.Path);

        var json = await File.ReadAllTextAsync(tempFile.Path);
        Assert.Contains("JetBrains Mono", json);
        Assert.Contains("#C9D1D9", json);
    }

    [Fact]
    public async Task ImportAsyncAppliesPreferencesAndRefreshesOpenDocument()
    {
        using var tempFile = new TempJsonFile();
        await File.WriteAllTextAsync(tempFile.Path, """
        {
          "LogFontFamilyName": "Cascadia Mono",
          "LogFontSize": 15,
          "IsStructuredMode": true
        }
        """);
        UserPreferences? imported = null;
        var viewportReloaded = false;
        var workflow = new SettingsTransferWorkflow(
            CreatePreferences,
            preferences => imported = preferences,
            () => Task.CompletedTask,
            () => true,
            () =>
            {
                viewportReloaded = true;
                return Task.CompletedTask;
            },
            _ => { });

        await workflow.ImportAsync(tempFile.Path);

        Assert.NotNull(imported);
        Assert.Equal("Cascadia Mono", imported.LogFontFamilyName);
        Assert.True(imported.IsStructuredMode);
        Assert.True(viewportReloaded);
    }

    private static UserPreferences CreatePreferences()
    {
        return new UserPreferences
        {
            LogFontFamilyName = "JetBrains Mono",
            LogFontSize = 14,
            LogTextColor = "#C9D1D9",
            HighlightRulesConfigured = true,
            HighlightRules = [new WorkspaceHighlightRule
            {
                Enabled = true,
                Pattern = "ERROR",
                IsRegex = false,
                Foreground = "#FFD6D6",
                Background = "#3A1618"
            }]
        };
    }

    private sealed class TempJsonFile : IDisposable
    {
        public TempJsonFile()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.needle-settings.json");
        }

        public string Path { get; }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}
