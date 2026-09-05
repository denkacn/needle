using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Tests;

public sealed class RecentFilesCoordinatorTests
{
    [Fact]
    public void AddMovesExistingFileToTop()
    {
        var recentFiles = new RecentFilesCoordinator(_ => Task.CompletedTask);

        recentFiles.Add("D:\\Logs\\one.log");
        recentFiles.Add("D:\\Logs\\two.log");
        recentFiles.Add("D:\\Logs\\one.log");

        Assert.Equal(["D:\\Logs\\one.log", "D:\\Logs\\two.log"], recentFiles.Paths);
    }

    [Fact]
    public void AddKeepsOnlyTenFiles()
    {
        var recentFiles = new RecentFilesCoordinator(_ => Task.CompletedTask);

        for (var i = 0; i < 12; i++)
        {
            recentFiles.Add($"D:\\Logs\\{i}.log");
        }

        Assert.Equal(10, recentFiles.Items.Count);
        Assert.Equal("D:\\Logs\\11.log", recentFiles.Items[0].Path);
        Assert.Equal("D:\\Logs\\2.log", recentFiles.Items[^1].Path);
    }

    [Fact]
    public void RestoreNormalizesAndDeduplicatesPaths()
    {
        var recentFiles = new RecentFilesCoordinator(_ => Task.CompletedTask);

        recentFiles.Restore(["D:\\Logs\\one.log", "", "D:\\Logs\\ONE.log", "D:\\Logs\\two.log"]);

        Assert.Equal(["D:\\Logs\\one.log", "D:\\Logs\\two.log"], recentFiles.Paths);
    }

    [Fact]
    public void RemoveReturnsFalseWhenFileDoesNotExistInRecentList()
    {
        var recentFiles = new RecentFilesCoordinator(_ => Task.CompletedTask);
        recentFiles.Add("D:\\Logs\\one.log");

        Assert.False(recentFiles.Remove("D:\\Logs\\missing.log"));
        Assert.True(recentFiles.Remove("D:\\Logs\\one.log"));
        Assert.False(recentFiles.HasItems);
    }
}
