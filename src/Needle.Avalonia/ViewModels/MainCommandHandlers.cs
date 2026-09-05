namespace Needle.Avalonia.ViewModels;

internal sealed record FileCommandHandlers(
    Func<Task> OpenFileAsync,
    Action ShowInExplorer,
    Action OpenInEditor,
    Func<Task> ClearLogAsync,
    Func<bool> CanUseActiveFile);

internal sealed record NavigationCommandHandlers(
    Action PageUp,
    Action PageDown,
    Action GoToTop,
    Action GoToEnd,
    Action ToggleFollowTail,
    Func<bool> CanNavigate,
    Func<bool> CanToggleFollowTail);

internal sealed record SearchCommandHandlers(
    Func<Task> SearchNextAsync,
    Action SearchPrevious,
    Func<Task> ApplyFilterAsync,
    Action ClearFilter,
    Func<bool> CanSearch,
    Func<bool> HasSearchMatches,
    Func<bool> CanApplyFilter,
    Func<bool> CanClearFilter);

internal sealed record BookmarkCommandHandlers(
    Action ToggleBookmark,
    Action GoToNextBookmark,
    Action GoToPreviousBookmark,
    Action RemoveSelectedBookmark,
    Action ClearBookmarks,
    Func<bool> HasBookmarks);

internal sealed record TriggerCommandHandlers(
    Action GoToPreviousTrigger,
    Action GoToNextTrigger,
    Action RemoveSelectedTrigger,
    Action ClearTriggers,
    Func<bool> HasTriggerHits);

internal sealed record TabCommandHandlers(
    Action<LogTabViewModel?> SelectTab,
    Action<LogTabViewModel?> CloseTab,
    Action CloseSelectedTab,
    Func<bool> HasSelectedTab);
