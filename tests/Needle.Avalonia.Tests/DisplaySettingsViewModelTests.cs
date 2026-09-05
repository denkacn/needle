namespace Needle.Avalonia.Tests;

using Needle.Avalonia.ViewModels;

public sealed class DisplaySettingsViewModelTests
{
    [Fact]
    public void FontOptionsAreNotEmpty()
    {
        var viewModel = new DisplaySettingsViewModel();

        Assert.NotEmpty(viewModel.LogFontFamilyOptions);
        Assert.Contains(viewModel.LogFontFamilyName, viewModel.LogFontFamilyOptions);
    }

    [Fact]
    public void UnknownFontFallsBackToAvailableOption()
    {
        var viewModel = new DisplaySettingsViewModel();
        var fallback = viewModel.LogFontFamilyOptions[0];

        viewModel.LogFontFamilyName = "Definitely Missing Log Font";

        Assert.Equal(fallback, viewModel.LogFontFamilyName);
    }

    [Fact]
    public void CustomFontCanBeAddedAndSelected()
    {
        var viewModel = new DisplaySettingsViewModel();
        var systemFont = LogFontOptionsProvider.GetSystemFontNames()
            .FirstOrDefault(font => !viewModel.LogFontFamilyOptions.Contains(font));

        if (systemFont is null)
        {
            return;
        }

        var added = viewModel.AddCustomLogFont(systemFont);

        Assert.True(added);
        Assert.Contains(systemFont, viewModel.LogFontFamilyOptions);
        Assert.Contains(systemFont, viewModel.CustomLogFontFamilyNames);
        Assert.Equal(systemFont, viewModel.LogFontFamilyName);
        Assert.True(viewModel.CanRemoveSelectedCustomLogFont);
    }

    [Fact]
    public void SelectedCustomFontCanBeRemoved()
    {
        var viewModel = new DisplaySettingsViewModel();
        var systemFont = LogFontOptionsProvider.GetSystemFontNames()
            .FirstOrDefault(font => !viewModel.LogFontFamilyOptions.Contains(font));

        if (systemFont is null)
        {
            return;
        }

        viewModel.AddCustomLogFont(systemFont);
        viewModel.RemoveSelectedCustomLogFont();

        Assert.DoesNotContain(systemFont, viewModel.LogFontFamilyOptions);
        Assert.DoesNotContain(systemFont, viewModel.CustomLogFontFamilyNames);
        Assert.False(viewModel.CanRemoveSelectedCustomLogFont);
    }

    [Fact]
    public void MiniMapIsEnabledByDefaultAndCanBeRestoredFromPreferences()
    {
        var viewModel = new DisplaySettingsViewModel();

        Assert.True(viewModel.IsMiniMapEnabled);

        viewModel.Apply(new()
        {
            IsMiniMapEnabled = false
        });

        Assert.False(viewModel.IsMiniMapEnabled);
    }
}
