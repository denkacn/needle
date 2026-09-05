namespace Needle.Avalonia.Views;

using Needle.Avalonia.ViewModels;

internal sealed class SettingsHighlightController
{
    private readonly Func<string?, Task<string?>> _pickColorAsync;

    public SettingsHighlightController(Func<string?, Task<string?>> pickColorAsync)
    {
        _pickColorAsync = pickColorAsync ?? throw new ArgumentNullException(nameof(pickColorAsync));
    }

    public async Task PickNewRuleForegroundAsync(HighlightRulesViewModel highlightRules)
    {
        var color = await _pickColorAsync(highlightRules.NewForeground);
        if (color is not null)
        {
            highlightRules.NewForeground = color;
        }
    }

    public async Task PickNewRuleBackgroundAsync(HighlightRulesViewModel highlightRules)
    {
        var color = await _pickColorAsync(highlightRules.NewBackground);
        if (color is not null)
        {
            highlightRules.NewBackground = color;
        }
    }

    public void ResetNewRuleBackground(HighlightRulesViewModel highlightRules)
    {
        highlightRules.NewBackground = null;
    }

    public async Task PickRuleForegroundAsync(HighlightRuleViewModel rule)
    {
        var color = await _pickColorAsync(rule.Foreground);
        if (color is not null)
        {
            rule.Foreground = color;
        }
    }

    public async Task PickRuleBackgroundAsync(HighlightRuleViewModel rule)
    {
        var color = await _pickColorAsync(rule.Background);
        if (color is not null)
        {
            rule.Background = color;
        }
    }

    public void ResetRuleBackground(HighlightRuleViewModel rule)
    {
        rule.Background = null;
    }
}

