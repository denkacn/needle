namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

public sealed class ExcludeRulesViewModel : ViewModelBase
{
    private string _pendingPattern = string.Empty;
    private string _status = string.Empty;

    public ExcludeRulesViewModel()
    {
        AddRuleCommand = new RelayCommand(AddPendingRule, CanAddPendingRule);
        ClearRulesCommand = new RelayCommand(Clear, () => HasRules);
    }

    public event Action? RulesChanged;

    public ObservableCollection<ExcludeRuleViewModel> Rules { get; } = [];

    public string PendingPattern
    {
        get => _pendingPattern;
        set
        {
            if (SetProperty(ref _pendingPattern, value))
            {
                AddRuleCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public bool HasRules => Rules.Count > 0;

    public string CountText => Rules.Count == 0 ? string.Empty : Rules.Count.ToString();

    public IRelayCommand AddRuleCommand { get; }

    public IRelayCommand ClearRulesCommand { get; }

    public IReadOnlyList<string> ToPatterns()
    {
        return [.. Rules.Select(rule => rule.Pattern)];
    }

    public void Restore(IEnumerable<string> patterns)
    {
        Rules.Clear();
        foreach (var pattern in Normalize(patterns))
        {
            Rules.Add(CreateRule(pattern));
        }

        RefreshState();
    }

    public void Clear()
    {
        if (Rules.Count == 0)
        {
            return;
        }

        Rules.Clear();
        RefreshState();
        RulesChanged?.Invoke();
    }

    private void AddPendingRule()
    {
        var pattern = PendingPattern.Trim();
        if (pattern.Length == 0 ||
            Rules.Any(rule => string.Equals(rule.Pattern, pattern, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        Rules.Add(CreateRule(pattern));
        PendingPattern = string.Empty;
        RefreshState();
        RulesChanged?.Invoke();
    }

    private bool CanAddPendingRule()
    {
        return !string.IsNullOrWhiteSpace(PendingPattern);
    }

    private ExcludeRuleViewModel CreateRule(string pattern)
    {
        return new ExcludeRuleViewModel(pattern, RemoveRule);
    }

    private void RemoveRule(ExcludeRuleViewModel rule)
    {
        if (!Rules.Remove(rule))
        {
            return;
        }

        RefreshState();
        RulesChanged?.Invoke();
    }

    private void RefreshState()
    {
        Status = Rules.Count switch
        {
            0 => string.Empty,
            1 => "1 exclude rule",
            _ => $"{Rules.Count} exclude rules"
        };

        OnPropertyChanged(nameof(HasRules));
        OnPropertyChanged(nameof(CountText));
        ClearRulesCommand.NotifyCanExecuteChanged();
    }

    private static IEnumerable<string> Normalize(IEnumerable<string> patterns)
    {
        return patterns
            .Select(pattern => pattern.Trim())
            .Where(pattern => pattern.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
