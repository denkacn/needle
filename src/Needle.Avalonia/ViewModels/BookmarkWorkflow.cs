namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Needle.Core.Bookmarks;

internal sealed class BookmarkWorkflow
{
    private readonly BookmarkSelectionWorkflow _bookmarks;
    private readonly DocumentStateViewModel _document;
    private readonly SearchFilterViewModel _searchFilter;
    private readonly ObservableCollection<long> _filteredLineNumbers;
    private readonly Func<long, long> _clampFirstVisibleLine;
    private readonly Action _reloadViewport;
    private readonly Action _saveWorkspace;
    private readonly Action<string> _notifyPropertyChanged;
    private readonly Func<BookmarkCommandRefreshContext> _getCommands;
    private readonly Action _refreshTimeline;

    public BookmarkWorkflow(
        BookmarkSelectionWorkflow bookmarks,
        DocumentStateViewModel document,
        SearchFilterViewModel searchFilter,
        ObservableCollection<long> filteredLineNumbers,
        Func<long, long> clampFirstVisibleLine,
        Action reloadViewport,
        Action saveWorkspace,
        Action<string> notifyPropertyChanged,
        Func<BookmarkCommandRefreshContext> getCommands,
        Action refreshTimeline)
    {
        _bookmarks = bookmarks;
        _document = document;
        _searchFilter = searchFilter;
        _filteredLineNumbers = filteredLineNumbers;
        _clampFirstVisibleLine = clampFirstVisibleLine;
        _reloadViewport = reloadViewport;
        _saveWorkspace = saveWorkspace;
        _notifyPropertyChanged = notifyPropertyChanged;
        _getCommands = getCommands;
        _refreshTimeline = refreshTimeline;
    }

    public void Toggle()
    {
        _bookmarks.Toggle(_document, _searchFilter.IsFilterActive, _filteredLineNumbers);
        RefreshAfterMutation();
    }

    public void GoToNext()
    {
        RefreshIfMoved(_bookmarks.TryGoToNext(_document, _searchFilter.IsFilterActive, _filteredLineNumbers, _clampFirstVisibleLine));
    }

    public void GoToPrevious()
    {
        RefreshIfMoved(_bookmarks.TryGoToPrevious(_document, _searchFilter.IsFilterActive, _filteredLineNumbers, _clampFirstVisibleLine));
    }

    public void RemoveSelected()
    {
        if (!_bookmarks.RemoveCurrent(_document, _searchFilter.IsFilterActive, _filteredLineNumbers, _clampFirstVisibleLine))
        {
            return;
        }

        RefreshAfterMutation();
    }

    public void Clear()
    {
        _bookmarks.Clear(_document);
        RefreshAfterMutation();
    }

    public void Restore(IEnumerable<long> lineNumbers)
    {
        _bookmarks.Restore(lineNumbers, _document);
        RefreshCommands();
    }

    public void RefreshCommands()
    {
        var commands = _getCommands();
        commands.Next.NotifyCanExecuteChanged();
        commands.Previous.NotifyCanExecuteChanged();
        commands.RemoveSelected.NotifyCanExecuteChanged();
        commands.Clear.NotifyCanExecuteChanged();
        _notifyPropertyChanged(nameof(MainViewModel.HasBookmarks));
        _notifyPropertyChanged(nameof(MainViewModel.SelectedBookmarkText));
        _refreshTimeline();
    }

    private void RefreshAfterMutation()
    {
        RefreshCommands();
        _reloadViewport();
        _saveWorkspace();
    }

    private void RefreshIfMoved(bool moved)
    {
        if (moved)
        {
            RefreshCommands();
        }
    }
}

internal sealed record BookmarkCommandRefreshContext(
    IRelayCommand Next,
    IRelayCommand Previous,
    IRelayCommand RemoveSelected,
    IRelayCommand Clear);
