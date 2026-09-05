using Avalonia.Controls;
using Avalonia.Interactivity;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Views;

public partial class HighlightRulesView : UserControl
{
    private readonly SettingsHighlightController _highlightController;

    public HighlightRulesView()
    {
        InitializeComponent();
        _highlightController = new SettingsHighlightController(PickColorAsync);
    }

    private void OnPickNewForegroundClicked(object? sender, RoutedEventArgs e)
    {
        AsyncEventRunner.Run(PickNewForegroundAsync);
    }

    private async Task PickNewForegroundAsync()
    {
        if (DataContext is HighlightRulesViewModel highlightRules)
        {
            await _highlightController.PickNewRuleForegroundAsync(highlightRules);
        }
    }

    private void OnPickNewBackgroundClicked(object? sender, RoutedEventArgs e)
    {
        AsyncEventRunner.Run(PickNewBackgroundAsync);
    }

    private async Task PickNewBackgroundAsync()
    {
        if (DataContext is HighlightRulesViewModel highlightRules)
        {
            await _highlightController.PickNewRuleBackgroundAsync(highlightRules);
        }
    }

    private void OnResetNewBackgroundClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is HighlightRulesViewModel highlightRules)
        {
            _highlightController.ResetNewRuleBackground(highlightRules);
        }
    }

    private void OnPickRuleForegroundClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: HighlightRuleViewModel rule })
        {
            AsyncEventRunner.Run(() => _highlightController.PickRuleForegroundAsync(rule));
        }
    }

    private void OnPickRuleBackgroundClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: HighlightRuleViewModel rule })
        {
            AsyncEventRunner.Run(() => _highlightController.PickRuleBackgroundAsync(rule));
        }
    }

    private void OnResetRuleBackgroundClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: HighlightRuleViewModel rule })
        {
            _highlightController.ResetRuleBackground(rule);
        }
    }

    private void OnRemoveRuleClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: HighlightRuleViewModel rule }
            && DataContext is HighlightRulesViewModel highlightRules)
        {
            highlightRules.RemoveRuleCommand.Execute(rule);
        }
    }

    private async Task<string?> PickColorAsync(string? initialColor)
    {
        var dialogs = WindowDialogServiceResolver.TryCreate(this);
        return dialogs is null ? null : await dialogs.PickColorAsync(initialColor);
    }
}
