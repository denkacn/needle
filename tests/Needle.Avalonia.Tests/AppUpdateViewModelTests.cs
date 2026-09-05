using Needle.Avalonia.Services;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Tests;

public sealed class AppUpdateViewModelTests
{
    [Fact]
    public async Task CheckForUpdatesShowsAvailableVersion()
    {
        var viewModel = new AppUpdateViewModel(
            new TestAppInfoService(),
            new TestAppUpdateService(new AppUpdateCheckResult(true, "0.8.4")));

        await viewModel.CheckForUpdatesAsync();

        Assert.True(viewModel.HasAvailableUpdate);
        Assert.Equal("v0.8.3", viewModel.CurrentVersionDisplay);
        Assert.Equal("v0.8.4", viewModel.AvailableVersionDisplay);
        Assert.Equal("v0.8.4 is available", viewModel.AvailableUpdateText);
    }

    [Fact]
    public async Task CheckForUpdatesHidesPromptWhenNoUpdateExists()
    {
        var viewModel = new AppUpdateViewModel(
            new TestAppInfoService(),
            new TestAppUpdateService(new AppUpdateCheckResult(false, null)));

        await viewModel.CheckForUpdatesAsync();

        Assert.False(viewModel.HasAvailableUpdate);
        Assert.Null(viewModel.AvailableVersionDisplay);
        Assert.Equal(string.Empty, viewModel.AvailableUpdateText);
    }

    private sealed class TestAppInfoService : IAppInfoService
    {
        public string Version => "0.8.3";

        public string DisplayVersion => "v0.8.3";
    }

    private sealed class TestAppUpdateService : IAppUpdateService
    {
        private readonly AppUpdateCheckResult _result;

        public TestAppUpdateService(AppUpdateCheckResult result)
        {
            _result = result;
        }

        public Task<AppUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }

        public Task DownloadAndApplyUpdateAsync(IProgress<int> progress, CancellationToken cancellationToken)
        {
            progress.Report(100);
            return Task.CompletedTask;
        }
    }
}
