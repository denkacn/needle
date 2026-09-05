using CommunityToolkit.Mvvm.ComponentModel;
using Needle.Application.Documents;
using Needle.Application.Workspace;
using Needle.Core.Bookmarks;
using Needle.Core.Search;

namespace Needle.Avalonia.ViewModels;

public sealed class LogTabViewModel : ObservableObject
{
    private string _title;
    private bool _isActive;

    internal LogTabViewModel(
        string path,
        LogDocumentSession session)
    {
        Path = path;
        _title = System.IO.Path.GetFileName(path);
        Runtime = new LogTabRuntime(session);
        Highlights.RulesChanged += (_, _) => OnPropertyChanged(nameof(HasCustomHighlights));
        Triggers.StateChanged += OnTriggersStateChanged;
    }

    public string Path { get; }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public HighlightRulesViewModel Highlights { get; } = new(includeDefaultRules: false);

    public TriggerRulesViewModel Triggers { get; } = new();

    public bool HasCustomHighlights => Highlights.HasRules;

    public bool HasTriggeredHits => Triggers.HasHits;

    internal LogTabRuntime Runtime { get; }

    internal LogDocumentSession Session => Runtime.Session;

    internal long FileSize
    {
        get => Runtime.FileSize;
        set => Runtime.FileSize = value;
    }

    internal double ScrollLine { get => Runtime.ScrollLine; set => Runtime.ScrollLine = value; }

    internal string SearchText { get => Runtime.SearchText; set => Runtime.SearchText = value; }

    internal bool IsSearchCaseSensitive { get => Runtime.IsSearchCaseSensitive; set => Runtime.IsSearchCaseSensitive = value; }

    internal bool IsSearchRegex { get => Runtime.IsSearchRegex; set => Runtime.IsSearchRegex = value; }

    internal string SearchStatus { get => Runtime.SearchStatus; set => Runtime.SearchStatus = value; }

    internal List<LogSearchMatch> SearchMatches => Runtime.SearchMatches;

    internal string FilterText { get => Runtime.FilterText; set => Runtime.FilterText = value; }

    internal string FilterStatus { get => Runtime.FilterStatus; set => Runtime.FilterStatus = value; }

    internal bool IsFilterActive { get => Runtime.IsFilterActive; set => Runtime.IsFilterActive = value; }

    internal List<long> FilteredLineNumbers => Runtime.FilteredLineNumbers;

    internal List<string> ExcludePatterns => Runtime.ExcludePatterns;

    internal List<string> TriggerPatterns => Runtime.TriggerPatterns;

    internal string ExcludeStatus { get => Runtime.ExcludeStatus; set => Runtime.ExcludeStatus = value; }

    internal List<LogBookmark> Bookmarks => Runtime.Bookmarks;

    internal string BookmarkStatus { get => Runtime.BookmarkStatus; set => Runtime.BookmarkStatus = value; }

    internal bool IsTailPaused { get => Runtime.IsTailPaused; set => Runtime.IsTailPaused = value; }

    internal bool IsFollowingTail { get => Runtime.IsFollowingTail; set => Runtime.IsFollowingTail = value; }

    internal bool IsStructuredMode { get => Runtime.IsStructuredMode; set => Runtime.IsStructuredMode = value; }

    internal long SelectedLineNumber { get => Runtime.SelectedLineNumber; set => Runtime.SelectedLineNumber = value; }

    internal string SelectedLineText { get => Runtime.SelectedLineText; set => Runtime.SelectedLineText = value; }

    internal void RestoreHighlightRules(bool configured, IEnumerable<WorkspaceHighlightRule> rules)
    {
        Highlights.Restore(configured, rules);
        OnPropertyChanged(nameof(HasCustomHighlights));
    }

    internal void SaveTransientState(LogTabTransientState state)
    {
        Runtime.Save(state);
    }

    private void OnTriggersStateChanged()
    {
        OnPropertyChanged(nameof(HasTriggeredHits));
    }
}
