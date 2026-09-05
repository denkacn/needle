using CommunityToolkit.Mvvm.ComponentModel;
using Needle.Core.Highlighting;

namespace Needle.Avalonia.ViewModels;

public sealed class HighlightRuleViewModel : ObservableObject
{
    private string _pattern;
    private bool _isRegex;
    private string? _foreground;
    private string? _background;
    private bool _enabled;

    public HighlightRuleViewModel(LogHighlightRule rule)
    {
        _pattern = rule.Pattern;
        _isRegex = rule.IsRegex;
        _foreground = rule.Foreground;
        _background = rule.Background;
        _enabled = rule.Enabled;
    }

    public string Pattern
    {
        get => _pattern;
        set => SetProperty(ref _pattern, value);
    }

    public bool IsRegex
    {
        get => _isRegex;
        set => SetProperty(ref _isRegex, value);
    }

    public string? Foreground
    {
        get => _foreground;
        set => SetProperty(ref _foreground, value);
    }

    public string? Background
    {
        get => _background;
        set
        {
            if (SetProperty(ref _background, string.IsNullOrWhiteSpace(value) ? null : value.Trim()))
            {
                OnPropertyChanged(nameof(BackgroundDisplay));
            }
        }
    }

    public string BackgroundDisplay => string.IsNullOrWhiteSpace(Background) ? "Default" : Background;

    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    public IReadOnlyList<string> ColorOptions => HighlightColorPalette.Values;

    public LogHighlightRule ToRule()
    {
        return new LogHighlightRule(
            Pattern,
            IsRegex,
            string.IsNullOrWhiteSpace(Foreground) ? null : Foreground.Trim(),
            string.IsNullOrWhiteSpace(Background) ? null : Background.Trim(),
            Enabled);
    }
}
