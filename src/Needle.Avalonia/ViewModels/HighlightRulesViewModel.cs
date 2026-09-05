namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using Needle.Application.Workspace;
using Needle.Core.Highlighting;

public sealed class HighlightRulesViewModel : ViewModelBase
{
    private string _status = string.Empty;
    private string _newPattern = string.Empty;
    private string _newForeground = "#CFE8FF";
    private string? _newBackground;
    private bool _newIsRegex;

    public HighlightRulesViewModel(bool includeDefaultRules = true)
    {
        AddRuleCommand = new RelayCommand(AddRule, () => !string.IsNullOrWhiteSpace(NewPattern));
        ClearRulesCommand = new RelayCommand(Clear, () => Rules.Count > 0);
        RemoveRuleCommand = new RelayCommand<HighlightRuleViewModel>(Remove);
        Rules.CollectionChanged += OnRulesCollectionChanged;
        if (includeDefaultRules)
        {
            AddDefaultRules();
        }

        RefreshStatus();
        ClearRulesCommand.NotifyCanExecuteChanged();
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string NewPattern
    {
        get => _newPattern;
        set
        {
            if (SetProperty(ref _newPattern, value))
            {
                AddRuleCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string NewForeground
    {
        get => _newForeground;
        set => SetProperty(ref _newForeground, NormalizeColor(value));
    }

    public string? NewBackground
    {
        get => _newBackground;
        set
        {
            if (SetProperty(ref _newBackground, NormalizeOptionalColor(value)))
            {
                OnPropertyChanged(nameof(NewBackgroundDisplay));
            }
        }
    }

    public string NewBackgroundDisplay => string.IsNullOrWhiteSpace(NewBackground) ? "Default" : NewBackground;

    public bool NewIsRegex
    {
        get => _newIsRegex;
        set => SetProperty(ref _newIsRegex, value);
    }

    public ObservableCollection<HighlightRuleViewModel> Rules { get; } = [];

    public bool HasRules => Rules.Count > 0;

    public IRelayCommand AddRuleCommand { get; }

    public IRelayCommand ClearRulesCommand { get; }

    public IRelayCommand<HighlightRuleViewModel> RemoveRuleCommand { get; }

    public event EventHandler? RulesChanged;

    public void AddUserRule(string pattern, bool isRegex, string? foreground, string? background)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        Rules.Insert(0, new HighlightRuleViewModel(new LogHighlightRule(
            pattern.Trim(),
            isRegex,
            Foreground: NormalizeOptionalColor(foreground),
            Background: NormalizeOptionalColor(background))));
        PublishChanged();
    }

    public void ResetNewBackground()
    {
        NewBackground = null;
    }

    public void Clear()
    {
        Rules.Clear();
        RefreshStatus();
        RulesChanged?.Invoke(this, EventArgs.Empty);
        ClearRulesCommand.NotifyCanExecuteChanged();
    }

    public void Remove(HighlightRuleViewModel? rule)
    {
        if (rule is null)
        {
            return;
        }

        Rules.Remove(rule);
        PublishChanged();
    }

    public void Restore(WorkspaceState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        Restore(state.HighlightRulesConfigured, state.HighlightRules);
    }

    public void Restore(bool configured, IEnumerable<WorkspaceHighlightRule> rules)
    {
        if (!configured)
        {
            return;
        }

        Rules.Clear();
        foreach (var rule in rules)
        {
            if (string.IsNullOrWhiteSpace(rule.Pattern))
            {
                continue;
            }

            Rules.Add(new HighlightRuleViewModel(new LogHighlightRule(
                rule.Pattern,
                rule.IsRegex,
                rule.Foreground,
                rule.Background,
                rule.Enabled)));
        }

        RefreshStatus();
        RulesChanged?.Invoke(this, EventArgs.Empty);
        ClearRulesCommand.NotifyCanExecuteChanged();
    }

    public List<WorkspaceHighlightRule> ToWorkspaceRules()
    {
        return [.. Rules.Select(rule => new WorkspaceHighlightRule
        {
            Pattern = rule.Pattern,
            IsRegex = rule.IsRegex,
            Foreground = rule.Foreground,
            Background = rule.Background,
            Enabled = rule.Enabled
        })];
    }

    public List<LogHighlightRule> ToRules()
    {
        return [.. Rules
            .Where(rule => !string.IsNullOrWhiteSpace(rule.Pattern))
            .Select(rule => rule.ToRule())];
    }

    private void AddDefaultRules()
    {
        Rules.Add(new(new("FATAL", IsRegex: false, Foreground: "#FFD6D1", Background: "#3A1618")));
        Rules.Add(new(new("ERROR", IsRegex: false, Foreground: "#FFD6D1", Background: "#3A1618")));
        Rules.Add(new(new("WARNING", IsRegex: false, Foreground: "#FFE1A3", Background: "#3A2D12")));
        Rules.Add(new(new("INFO", IsRegex: false, Foreground: null, Background: null)));
    }

    private void AddRule()
    {
        if (string.IsNullOrWhiteSpace(NewPattern))
        {
            return;
        }

        AddUserRule(NewPattern, NewIsRegex, NewForeground, NewBackground);
        NewPattern = string.Empty;
    }

    private void PublishChanged()
    {
        RefreshStatus();
        RulesChanged?.Invoke(this, EventArgs.Empty);
        ClearRulesCommand.NotifyCanExecuteChanged();
    }

    private void RefreshStatus()
    {
        Status = Rules.Count == 0 ? "Highlight rules cleared" : $"{Rules.Count:N0} highlight rules";
        OnPropertyChanged(nameof(HasRules));
    }

    private void OnRulesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (HighlightRuleViewModel rule in e.OldItems)
            {
                rule.PropertyChanged -= OnRulePropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (HighlightRuleViewModel rule in e.NewItems)
            {
                rule.PropertyChanged += OnRulePropertyChanged;
            }
        }
    }

    private void OnRulePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        PublishChanged();
    }

    private static string NormalizeColor(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "#CFE8FF" : value.Trim();
    }

    private static string? NormalizeOptionalColor(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
