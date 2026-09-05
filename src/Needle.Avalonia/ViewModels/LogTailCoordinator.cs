namespace Needle.Avalonia.ViewModels;

using Needle.Application.Documents;
using Needle.Application.Tailing;

internal sealed class LogTailCoordinator
{
    public async Task RunAsync(
        LogDocumentSession session,
        Func<bool> isPaused,
        Func<ILogTailService?> getTailService,
        Func<LogTailUpdate, Task> onUpdateAsync,
        CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            var tailService = getTailService();
            if (isPaused() || tailService is null)
            {
                continue;
            }

            var update = await tailService.PollAsync(cancellationToken);
            if (update.WasReset || update.HasNewLines)
            {
                session.FileSize = update.CurrentLength;
                await onUpdateAsync(new LogTailUpdate(
                    update.WasReset,
                    update.HasNewLines,
                    update.AddedLineCount,
                    update.ResetReason));
            }
        }
    }
}

internal sealed record LogTailUpdate(
    bool WasReset,
    bool HasNewLines,
    int AddedLineCount,
    string? ResetReason);
