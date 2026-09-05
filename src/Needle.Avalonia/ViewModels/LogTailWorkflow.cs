namespace Needle.Avalonia.ViewModels;

using Needle.Application.Documents;

internal sealed class LogTailWorkflow
{
    private readonly LogTailCoordinator _tail = new();

    public void Toggle(DocumentStateViewModel document, Action goToEnd)
    {
        if (!document.IsDocumentOpen)
        {
            return;
        }

        document.IsFollowingTail = !document.IsFollowingTail;
        RefreshDisplayState(document);
        if (document.IsFollowingTail)
        {
            goToEnd();
        }
    }

    public void UpdateAutoFollowTail(
        DocumentStateViewModel document,
        DisplaySettingsViewModel displaySettings,
        double scrollLine)
    {
        if (!displaySettings.IsAutoFollowTailEnabled || !document.IsDocumentOpen)
        {
            return;
        }

        var shouldFollowTail = scrollLine >= document.MaximumScrollLine - 0.5;
        if (document.IsFollowingTail == shouldFollowTail)
        {
            return;
        }

        document.IsFollowingTail = shouldFollowTail;
        RefreshDisplayState(document);
    }

    public void RefreshDisplayState(DocumentStateViewModel document)
    {
        document.FollowTailButtonText = document.IsFollowingTail ? "Follow Tail: On" : "Follow Tail: Off";
    }

    public async Task RunAsync(
        LogDocumentSession? session,
        Func<LogDocumentSession?> getActiveSession,
        Func<Task> loadViewportFromScrollAsync,
        Action refreshMaximumScrollLine,
        Func<long, long> clampFirstVisibleLine,
        Func<LogDocumentSession, LogTailUpdate, CancellationToken, Task> processTailUpdateAsync,
        DocumentStateViewModel document,
        CancellationToken cancellationToken)
    {
        try
        {
            if (session is null)
            {
                return;
            }

            await _tail.RunAsync(
                session,
                () => document.IsTailPaused,
                () => getActiveSession()?.TailService,
                update => ApplyUpdateAsync(
                    update,
                    getActiveSession,
                    loadViewportFromScrollAsync,
                    refreshMaximumScrollLine,
                    clampFirstVisibleLine,
                    processTailUpdateAsync,
                    document,
                    cancellationToken),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static async Task ApplyUpdateAsync(
        LogTailUpdate update,
        Func<LogDocumentSession?> getActiveSession,
        Func<Task> loadViewportFromScrollAsync,
        Action refreshMaximumScrollLine,
        Func<long, long> clampFirstVisibleLine,
        Func<LogDocumentSession, LogTailUpdate, CancellationToken, Task> processTailUpdateAsync,
        DocumentStateViewModel document,
        CancellationToken cancellationToken)
    {
        var activeSession = getActiveSession();
        if (activeSession is null)
        {
            return;
        }

        await processTailUpdateAsync(activeSession, update, cancellationToken);
        refreshMaximumScrollLine();
        if (update.WasReset)
        {
            document.ScrollLine = document.IsFollowingTail
                ? document.MaximumScrollLine
                : clampFirstVisibleLine(ToLineNumber(document.ScrollLine));
            await loadViewportFromScrollAsync();
            document.Status = LogStatusFormatter.TailReset(update, activeSession.LineCount);
            return;
        }

        if (document.IsFollowingTail)
        {
            var previousScrollLine = document.ScrollLine;
            document.ScrollLine = document.MaximumScrollLine;
            if (Math.Abs(previousScrollLine - document.ScrollLine) < 0.001)
            {
                await loadViewportFromScrollAsync();
            }
        }
        else
        {
            document.Status = LogStatusFormatter.TailAdded(update, activeSession.LineCount);
        }
    }

    private static long ToLineNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 0;
        }

        return Math.Max(0, (long)Math.Round(value));
    }
}
