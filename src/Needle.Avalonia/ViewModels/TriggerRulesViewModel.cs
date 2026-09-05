namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using Needle.Core.Lines;

public sealed class TriggerRulesViewModel : ViewModelBase
{
    private string _pendingPattern = string.Empty;
    private string _status = string.Empty;
    private int _selectedHitIndex = -1;

    public TriggerRulesViewModel()
    {
        AddRuleCommand = new RelayCommand(AddPendingRule, CanAddPendingRule);
        ClearRulesCommand = new RelayCommand(ClearRules, () => HasRules);
        PreviousHitCommand = new RelayCommand(GoToPreviousHit, () => HasHits);
        NextHitCommand = new RelayCommand(GoToNextHit, () => HasHits);
        RemoveSelectedHitCommand = new RelayCommand(RemoveSelectedHit, () => SelectedHit is not null);
        ClearHitsCommand = new RelayCommand(ClearHits, () => HasHits);
        Rules.CollectionChanged += OnRulesCollectionChanged;
    }

    public event Action? StateChanged;

    public event Action<long>? NavigationRequested;

    public ObservableCollection<TriggerRuleViewModel> Rules { get; } = [];

    public ObservableCollection<TriggerHitViewModel> Hits { get; } = [];

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

    public int SelectedHitIndex
    {
        get => _selectedHitIndex;
        private set
        {
            if (SetProperty(ref _selectedHitIndex, value))
            {
                OnPropertyChanged(nameof(SelectedHit));
                RemoveSelectedHitCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public TriggerHitViewModel? SelectedHit =>
        SelectedHitIndex >= 0 && SelectedHitIndex < Hits.Count ? Hits[SelectedHitIndex] : null;

    public bool HasRules => Rules.Count > 0;

    public bool HasHits => Hits.Count > 0;

    public string CountText => Hits.Count == 0 ? string.Empty : Hits.Count.ToString("N0");

    public IRelayCommand AddRuleCommand { get; }

    public IRelayCommand ClearRulesCommand { get; }

    public IRelayCommand PreviousHitCommand { get; }

    public IRelayCommand NextHitCommand { get; }

    public IRelayCommand RemoveSelectedHitCommand { get; }

    public IRelayCommand ClearHitsCommand { get; }

    public IReadOnlyList<string> ToPatterns()
    {
        return [.. Rules.Select(rule => rule.Pattern)];
    }

    public void RestoreRules(IEnumerable<string> patterns)
    {
        Rules.Clear();
        foreach (var pattern in Normalize(patterns))
        {
            Rules.Add(CreateRule(pattern));
        }

        RefreshState();
    }

    public void ScanNewLines(IEnumerable<LogEntry> entries)
    {
        if (Rules.Count == 0)
        {
            return;
        }

        var rules = Rules
            .Select(rule => rule.Pattern.Trim())
            .Where(pattern => pattern.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var changed = false;

        foreach (var entry in entries)
        {
            foreach (var pattern in rules)
            {
                if (!entry.Text.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
                    Hits.Any(hit => hit.LineNumber == entry.Reference.LineNumber &&
                        string.Equals(hit.Pattern, pattern, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                Hits.Add(new TriggerHitViewModel(entry.Reference.LineNumber, pattern, entry.Text));
                changed = true;
            }
        }

        if (changed)
        {
            SelectedHitIndex = Hits.Count - 1;
            RefreshState();
        }
    }

    public bool ContainsHit(long lineNumber)
    {
        return Hits.Any(hit => hit.LineNumber == lineNumber);
    }

    public void ClearHits()
    {
        if (Hits.Count == 0)
        {
            return;
        }

        Hits.Clear();
        SelectedHitIndex = -1;
        RefreshState();
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
    }

    private void ClearRules()
    {
        if (Rules.Count == 0)
        {
            return;
        }

        Rules.Clear();
        ClearHits();
        RefreshState();
    }

    private void RemoveRule(TriggerRuleViewModel rule)
    {
        if (!Rules.Remove(rule))
        {
            return;
        }

        for (var i = Hits.Count - 1; i >= 0; i--)
        {
            if (string.Equals(Hits[i].Pattern, rule.Pattern, StringComparison.OrdinalIgnoreCase))
            {
                Hits.RemoveAt(i);
            }
        }

        if (SelectedHitIndex >= Hits.Count)
        {
            SelectedHitIndex = Hits.Count - 1;
        }

        RefreshState();
    }

    private void GoToPreviousHit()
    {
        if (Hits.Count == 0)
        {
            return;
        }

        SelectedHitIndex = SelectedHitIndex <= 0 ? Hits.Count - 1 : SelectedHitIndex - 1;
        RequestSelectedHitNavigation();
    }

    private void GoToNextHit()
    {
        if (Hits.Count == 0)
        {
            return;
        }

        SelectedHitIndex = SelectedHitIndex < 0 || SelectedHitIndex >= Hits.Count - 1 ? 0 : SelectedHitIndex + 1;
        RequestSelectedHitNavigation();
    }

    private void RemoveSelectedHit()
    {
        if (SelectedHit is null)
        {
            return;
        }

        Hits.RemoveAt(SelectedHitIndex);
        SelectedHitIndex = Hits.Count == 0 ? -1 : Math.Min(SelectedHitIndex, Hits.Count - 1);
        RefreshState();
        RequestSelectedHitNavigation();
    }

    private void RequestSelectedHitNavigation()
    {
        if (SelectedHit is not null)
        {
            NavigationRequested?.Invoke(SelectedHit.LineNumber);
        }
    }

    private bool CanAddPendingRule()
    {
        return !string.IsNullOrWhiteSpace(PendingPattern);
    }

    private TriggerRuleViewModel CreateRule(string pattern)
    {
        return new TriggerRuleViewModel(pattern, RemoveRule);
    }

    private void RefreshState()
    {
        Status = Rules.Count switch
        {
            0 => "No trigger rules",
            1 => Hits.Count == 0 ? "1 trigger rule" : $"1 trigger rule, {Hits.Count:N0} hits",
            _ => Hits.Count == 0 ? $"{Rules.Count:N0} trigger rules" : $"{Rules.Count:N0} trigger rules, {Hits.Count:N0} hits"
        };

        OnPropertyChanged(nameof(HasRules));
        OnPropertyChanged(nameof(HasHits));
        OnPropertyChanged(nameof(CountText));
        OnPropertyChanged(nameof(SelectedHit));
        AddRuleCommand.NotifyCanExecuteChanged();
        ClearRulesCommand.NotifyCanExecuteChanged();
        PreviousHitCommand.NotifyCanExecuteChanged();
        NextHitCommand.NotifyCanExecuteChanged();
        RemoveSelectedHitCommand.NotifyCanExecuteChanged();
        ClearHitsCommand.NotifyCanExecuteChanged();
        StateChanged?.Invoke();
    }

    private void OnRulesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (TriggerRuleViewModel rule in e.OldItems)
            {
                rule.PropertyChanged -= OnRulePropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (TriggerRuleViewModel rule in e.NewItems)
            {
                rule.PropertyChanged += OnRulePropertyChanged;
            }
        }
    }

    private void OnRulePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RefreshState();
    }

    private static IEnumerable<string> Normalize(IEnumerable<string> patterns)
    {
        return patterns
            .Select(pattern => pattern.Trim())
            .Where(pattern => pattern.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
