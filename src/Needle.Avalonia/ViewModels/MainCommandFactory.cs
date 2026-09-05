namespace Needle.Avalonia.ViewModels;

internal sealed class MainCommandFactory
{
    public MainCommandSet Create(MainCommandContext context)
    {
        return new MainCommandSet(
            new FileCommandHandlers(
                context.OpenFileAsync,
                context.ShowActiveFileInExplorer,
                context.OpenActiveFileInEditor,
                context.ClearActiveLogAsync,
                context.CanOpenActiveFileExternally),
            new NavigationCommandHandlers(
                context.PageUp,
                context.PageDown,
                context.GoToTop,
                context.GoToEnd,
                context.ToggleFollowTail,
                context.CanNavigate,
                context.CanToggleFollowTail),
            new SearchCommandHandlers(
                context.SearchNextAsync,
                context.SearchPrevious,
                context.ApplyFilterAsync,
                context.ClearFilter,
                context.CanSearch,
                context.HasSearchMatches,
                context.CanApplyFilter,
                context.CanClearFilter),
            new BookmarkCommandHandlers(
                context.ToggleBookmark,
                context.GoToNextBookmark,
                context.GoToPreviousBookmark,
                context.RemoveSelectedBookmark,
                context.ClearBookmarks,
                context.HasBookmarks),
            new TriggerCommandHandlers(
                context.GoToPreviousTrigger,
                context.GoToNextTrigger,
                context.RemoveSelectedTrigger,
                context.ClearTriggers,
                context.HasTriggerHits),
            new TabCommandHandlers(
                context.SelectTab,
                context.CloseTab,
                context.CloseSelectedTab,
                context.HasSelectedTab));
    }
}

internal sealed record MainCommandContext(
    Func<Task> OpenFileAsync,
    Action ShowActiveFileInExplorer,
    Action OpenActiveFileInEditor,
    Func<Task> ClearActiveLogAsync,
    Action PageUp,
    Action PageDown,
    Action GoToTop,
    Action GoToEnd,
    Action ToggleFollowTail,
    Func<Task> SearchNextAsync,
    Action SearchPrevious,
    Func<Task> ApplyFilterAsync,
    Action ClearFilter,
    Action ToggleBookmark,
    Action GoToNextBookmark,
    Action GoToPreviousBookmark,
    Action RemoveSelectedBookmark,
    Action ClearBookmarks,
    Action GoToPreviousTrigger,
    Action GoToNextTrigger,
    Action RemoveSelectedTrigger,
    Action ClearTriggers,
    Action<LogTabViewModel?> SelectTab,
    Action<LogTabViewModel?> CloseTab,
    Action CloseSelectedTab,
    Func<bool> CanOpenActiveFileExternally,
    Func<bool> CanNavigate,
    Func<bool> CanToggleFollowTail,
    Func<bool> CanSearch,
    Func<bool> HasSearchMatches,
    Func<bool> CanApplyFilter,
    Func<bool> CanClearFilter,
    Func<bool> HasBookmarks,
    Func<bool> HasTriggerHits,
    Func<bool> HasSelectedTab);
