namespace Needle.Avalonia.ViewModels;

using Needle.Application.Workspace;

internal sealed class WorkspaceRestoreWorkflow
{
    public async Task RestoreAsync(WorkspaceRestoreContext context)
    {
        var preferencesRestored = await context.RestorePreferencesAsync();
        var state = await context.LoadWorkspaceAsync();
        if (state is null)
        {
            return;
        }

        if (!preferencesRestored)
        {
            context.RestoreLegacyPreferences(state);
            context.RestoreWorkspaceHighlightRules(state);
        }

        var documentStates = state.Documents.Where(document => File.Exists(document.Path)).ToArray();
        if (documentStates.Length == 0)
        {
            return;
        }

        context.BeginRestore();
        try
        {
            foreach (var documentState in documentStates)
            {
                await context.OpenFilePathAsync(documentState.Path);
                if (!context.Document.IsDocumentOpen || !context.HasActiveTab())
                {
                    continue;
                }

                context.SearchFilter.SearchText = documentState.SearchText;
                context.SearchFilter.FilterText = documentState.FilterText;
                context.ExcludeRules.Restore(documentState.ExcludePatterns);
                context.RestoreActiveTabTriggers(documentState.TriggerPatterns);
                context.RestoreActiveTabHighlightRules(documentState.HighlightRulesConfigured, documentState.HighlightRules);
                context.Document.IsTailPaused = documentState.IsTailPaused;
                context.Document.IsFollowingTail = documentState.IsFollowingTail;
                context.RefreshTailDisplayState();
                context.Document.IsStructuredMode = documentState.IsStructuredMode;
                context.RestoreBookmarks(documentState.Bookmarks);

                if ((documentState.IsFilterActive && !string.IsNullOrWhiteSpace(documentState.FilterText)) ||
                    documentState.ExcludePatterns.Count > 0)
                {
                    await context.ApplyFilterAsync();
                }

                context.Document.ScrollLine = context.ClampFirstVisibleLine(documentState.FirstVisibleLine);
                await context.LoadViewportFromScrollAsync();
                context.SaveActiveTabState();
            }

            context.SelectRestoredActiveTab(state.ActiveDocumentPath);
        }
        finally
        {
            context.EndRestore();
            context.QueueWorkspaceSave();
        }
    }
}

internal sealed record WorkspaceRestoreContext(
    DocumentStateViewModel Document,
    SearchFilterViewModel SearchFilter,
    ExcludeRulesViewModel ExcludeRules,
    Func<Task<bool>> RestorePreferencesAsync,
    Func<Task<WorkspaceState?>> LoadWorkspaceAsync,
    Func<string, Task> OpenFilePathAsync,
    Func<bool> HasActiveTab,
    Action BeginRestore,
    Action EndRestore,
    Action<WorkspaceState> RestoreLegacyPreferences,
    Action<WorkspaceState> RestoreWorkspaceHighlightRules,
    Action<bool, IEnumerable<WorkspaceHighlightRule>> RestoreActiveTabHighlightRules,
    Action<IEnumerable<string>> RestoreActiveTabTriggers,
    Action RefreshTailDisplayState,
    Action<IEnumerable<long>> RestoreBookmarks,
    Func<Task> ApplyFilterAsync,
    Func<long, long> ClampFirstVisibleLine,
    Func<Task> LoadViewportFromScrollAsync,
    Action SaveActiveTabState,
    Action<string?> SelectRestoredActiveTab,
    Action QueueWorkspaceSave);
