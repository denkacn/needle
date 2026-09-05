namespace Needle.Infrastructure.Tests;

using Needle.Application.Preferences;
using Needle.Application.Workspace;
using Needle.Infrastructure.Preferences;

public sealed class JsonUserPreferencesStoreTests
{
    [Fact]
    public async Task LoadAsyncReturnsNullWhenFileDoesNotExist()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "preferences.json");
        var store = new JsonUserPreferencesStore(path);

        var preferences = await store.LoadAsync();

        Assert.Null(preferences);
    }

    [Fact]
    public async Task SaveAndLoadRoundTripsPreferences()
    {
        var directory = Directory.CreateTempSubdirectory("needle-preferences-");
        try
        {
            var path = Path.Combine(directory.FullName, "preferences.json");
            var store = new JsonUserPreferencesStore(path);
            var expected = new UserPreferences
            {
                LogFontFamilyName = "Consolas",
                LogFontSize = 15,
                LogTextColor = "#24292F",
                IsAlwaysOnTop = true,
                IsAutoFollowTailEnabled = false,
                IsStructuredMode = true,
                RecentFiles = ["D:\\Logs\\one.log", "D:\\Logs\\two.log"],
                HighlightRulesConfigured = true,
                HighlightRules =
                [
                    new WorkspaceHighlightRule
                    {
                        Pattern = "ERROR",
                        Foreground = "#FFD6D1",
                        Background = null
                    }
                ],
                MainWindow = new WindowPreference { X = 64, Y = 72, Width = 1200, Height = 800 },
                SettingsWindow = new WindowPreference { Width = 700, Height = 500 }
            };

            await store.SaveAsync(expected);
            var actual = await store.LoadAsync();

            Assert.NotNull(actual);
            Assert.Equal("Consolas", actual.LogFontFamilyName);
            Assert.Equal(15, actual.LogFontSize);
            Assert.Equal("#24292F", actual.LogTextColor);
            Assert.True(actual.IsAlwaysOnTop);
            Assert.False(actual.IsAutoFollowTailEnabled);
            Assert.True(actual.IsStructuredMode);
            Assert.Equal(["D:\\Logs\\one.log", "D:\\Logs\\two.log"], actual.RecentFiles);
            Assert.True(actual.HighlightRulesConfigured);
            Assert.Equal("ERROR", Assert.Single(actual.HighlightRules).Pattern);
            Assert.Equal(64, actual.MainWindow.X!.Value);
            Assert.Equal(72, actual.MainWindow.Y!.Value);
            Assert.Equal(1200, actual.MainWindow.Width);
            Assert.Equal(500, actual.SettingsWindow.Height);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
