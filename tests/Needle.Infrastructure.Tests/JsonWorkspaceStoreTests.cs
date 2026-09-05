using Needle.Application.Workspace;
using Needle.Infrastructure.Workspace;

namespace Needle.Infrastructure.Tests;

public sealed class JsonWorkspaceStoreTests
{
    [Fact]
    public async Task LoadAsyncReturnsNullWhenFileDoesNotExist()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "workspace.json");
        var store = new JsonWorkspaceStore(path);

        var state = await store.LoadAsync();

        Assert.Null(state);
    }

    [Fact]
    public async Task SaveAndLoadRoundTripsWorkspaceState()
    {
        var directory = Directory.CreateTempSubdirectory("needle-workspace-");
        try
        {
            var path = Path.Combine(directory.FullName, "workspace.json");
            var store = new JsonWorkspaceStore(path);
            var expected = new WorkspaceState
            {
                Documents =
                [
                    new WorkspaceDocumentState
                    {
                        Path = "sample.log",
                        FirstVisibleLine = 42,
                        SearchText = "error",
                        FilterText = "+warn -debug",
                    IsFilterActive = true,
                    IsTailPaused = true,
                    IsFollowingTail = false,
                    IsStructuredMode = true,
                    Bookmarks = [4, 8, 15]
                    }
                ]
            };

            await store.SaveAsync(expected);
            var actual = await store.LoadAsync();

            Assert.NotNull(actual);
            var document = Assert.Single(actual.Documents);
            Assert.Equal("sample.log", document.Path);
            Assert.Equal(42, document.FirstVisibleLine);
            Assert.Equal("error", document.SearchText);
            Assert.Equal("+warn -debug", document.FilterText);
            Assert.True(document.IsFilterActive);
            Assert.True(document.IsTailPaused);
            Assert.False(document.IsFollowingTail);
            Assert.True(document.IsStructuredMode);
            Assert.Equal([4, 8, 15], document.Bookmarks);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAndLoadRoundTripsMultipleDocumentsAndActivePath()
    {
        var directory = Directory.CreateTempSubdirectory("needle-workspace-");
        try
        {
            var path = Path.Combine(directory.FullName, "workspace.json");
            var store = new JsonWorkspaceStore(path);
            var expected = new WorkspaceState
            {
                ActiveDocumentPath = "client.log",
                HighlightRulesConfigured = true,
                HighlightRules =
                [
                    new WorkspaceHighlightRule
                    {
                        Pattern = "Timeout",
                        Background = "#123456"
                    }
                ],
                Documents =
                [
                    new WorkspaceDocumentState { Path = "server.log", FirstVisibleLine = 10 },
                    new WorkspaceDocumentState { Path = "client.log", FirstVisibleLine = 20 }
                ]
            };

            await store.SaveAsync(expected);
            var actual = await store.LoadAsync();

            Assert.NotNull(actual);
            Assert.Equal("client.log", actual.ActiveDocumentPath);
            var highlight = Assert.Single(actual.HighlightRules);
            Assert.Equal("Timeout", highlight.Pattern);
            Assert.Equal("#123456", highlight.Background);
            Assert.Equal(["server.log", "client.log"], actual.Documents.Select(document => document.Path));
            Assert.Equal([10, 20], actual.Documents.Select(document => document.FirstVisibleLine));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsyncReturnsNullForInvalidJson()
    {
        var directory = Directory.CreateTempSubdirectory("needle-workspace-");
        try
        {
            var path = Path.Combine(directory.FullName, "workspace.json");
            await File.WriteAllTextAsync(path, "{not-json");
            var store = new JsonWorkspaceStore(path);

            var state = await store.LoadAsync();

            Assert.Null(state);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
