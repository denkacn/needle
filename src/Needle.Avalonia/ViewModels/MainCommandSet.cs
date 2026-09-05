namespace Needle.Avalonia.ViewModels;

using CommunityToolkit.Mvvm.Input;

internal sealed class MainCommandSet
{
    public MainCommandSet(
        FileCommandHandlers files,
        NavigationCommandHandlers navigation,
        SearchCommandHandlers search,
        BookmarkCommandHandlers bookmarks,
        TriggerCommandHandlers triggers,
        TabCommandHandlers tabs)
    {
        OpenFileCommand = new AsyncRelayCommand(files.OpenFileAsync);
        ShowInExplorerCommand = new RelayCommand(files.ShowInExplorer, files.CanUseActiveFile);
        OpenInEditorCommand = new RelayCommand(files.OpenInEditor, files.CanUseActiveFile);
        ClearLogCommand = new AsyncRelayCommand(files.ClearLogAsync, files.CanUseActiveFile);

        PageUpCommand = new RelayCommand(navigation.PageUp, navigation.CanNavigate);
        PageDownCommand = new RelayCommand(navigation.PageDown, navigation.CanNavigate);
        GoToTopCommand = new RelayCommand(navigation.GoToTop, navigation.CanNavigate);
        GoToEndCommand = new RelayCommand(navigation.GoToEnd, navigation.CanNavigate);
        ToggleFollowTailCommand = new RelayCommand(navigation.ToggleFollowTail, navigation.CanToggleFollowTail);

        SearchNextCommand = new AsyncRelayCommand(search.SearchNextAsync, search.CanSearch);
        SearchPreviousCommand = new RelayCommand(search.SearchPrevious, search.HasSearchMatches);
        ApplyFilterCommand = new AsyncRelayCommand(search.ApplyFilterAsync, search.CanApplyFilter);
        ClearFilterCommand = new RelayCommand(search.ClearFilter, search.CanClearFilter);

        ToggleBookmarkCommand = new RelayCommand(bookmarks.ToggleBookmark, navigation.CanNavigate);
        NextBookmarkCommand = new RelayCommand(bookmarks.GoToNextBookmark, bookmarks.HasBookmarks);
        PreviousBookmarkCommand = new RelayCommand(bookmarks.GoToPreviousBookmark, bookmarks.HasBookmarks);
        RemoveSelectedBookmarkCommand = new RelayCommand(bookmarks.RemoveSelectedBookmark, bookmarks.HasBookmarks);
        ClearBookmarksCommand = new RelayCommand(bookmarks.ClearBookmarks, bookmarks.HasBookmarks);

        PreviousTriggerCommand = new RelayCommand(triggers.GoToPreviousTrigger, triggers.HasTriggerHits);
        NextTriggerCommand = new RelayCommand(triggers.GoToNextTrigger, triggers.HasTriggerHits);
        RemoveSelectedTriggerCommand = new RelayCommand(triggers.RemoveSelectedTrigger, triggers.HasTriggerHits);
        ClearTriggersCommand = new RelayCommand(triggers.ClearTriggers, triggers.HasTriggerHits);

        SelectTabCommand = new RelayCommand<LogTabViewModel>(tabs.SelectTab);
        CloseTabCommand = new RelayCommand<LogTabViewModel>(tabs.CloseTab);
        CloseSelectedTabCommand = new RelayCommand(tabs.CloseSelectedTab, tabs.HasSelectedTab);

        DocumentStateCommands =
        [
            ShowInExplorerCommand,
            OpenInEditorCommand,
            ClearLogCommand,
            PageUpCommand,
            PageDownCommand,
            GoToTopCommand,
            GoToEndCommand,
            ToggleFollowTailCommand,
            SearchNextCommand,
            SearchPreviousCommand,
            ApplyFilterCommand,
            ClearFilterCommand,
            ToggleBookmarkCommand,
            NextBookmarkCommand,
            PreviousBookmarkCommand,
            RemoveSelectedBookmarkCommand,
            ClearBookmarksCommand,
            PreviousTriggerCommand,
            NextTriggerCommand,
            RemoveSelectedTriggerCommand,
            ClearTriggersCommand,
            CloseSelectedTabCommand
        ];
    }

    public IAsyncRelayCommand OpenFileCommand { get; }
    public IRelayCommand ShowInExplorerCommand { get; }
    public IRelayCommand OpenInEditorCommand { get; }
    public IAsyncRelayCommand ClearLogCommand { get; }
    public IRelayCommand PageUpCommand { get; }
    public IRelayCommand PageDownCommand { get; }
    public IRelayCommand GoToTopCommand { get; }
    public IRelayCommand GoToEndCommand { get; }
    public IRelayCommand ToggleFollowTailCommand { get; }
    public IAsyncRelayCommand SearchNextCommand { get; }
    public IRelayCommand SearchPreviousCommand { get; }
    public IAsyncRelayCommand ApplyFilterCommand { get; }
    public IRelayCommand ClearFilterCommand { get; }
    public IRelayCommand ToggleBookmarkCommand { get; }
    public IRelayCommand NextBookmarkCommand { get; }
    public IRelayCommand PreviousBookmarkCommand { get; }
    public IRelayCommand RemoveSelectedBookmarkCommand { get; }
    public IRelayCommand ClearBookmarksCommand { get; }
    public IRelayCommand PreviousTriggerCommand { get; }
    public IRelayCommand NextTriggerCommand { get; }
    public IRelayCommand RemoveSelectedTriggerCommand { get; }
    public IRelayCommand ClearTriggersCommand { get; }
    public IRelayCommand<LogTabViewModel> SelectTabCommand { get; }
    public IRelayCommand<LogTabViewModel> CloseTabCommand { get; }
    public IRelayCommand CloseSelectedTabCommand { get; }
    public IReadOnlyList<IRelayCommand> DocumentStateCommands { get; }

    public void NotifyDocumentStateCanExecuteChanged()
    {
        foreach (var command in DocumentStateCommands)
        {
            command.NotifyCanExecuteChanged();
        }
    }
}
