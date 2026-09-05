namespace Needle.Avalonia.ViewModels;

using Needle.Application.Documents;
using Needle.Application.Tailing;

internal sealed class TriggerTailScanner
{
    public async Task ScanAsync(
        LogDocumentSession session,
        TriggerRulesViewModel triggers,
        LogTailUpdate update,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(triggers);
        ArgumentNullException.ThrowIfNull(update);

        if (update.WasReset)
        {
            triggers.ClearHits();
        }

        if (!update.HasNewLines || !triggers.HasRules)
        {
            return;
        }

        var count = Math.Min(update.AddedLineCount, int.MaxValue);
        var startLine = Math.Max(0, session.LineCount - count);
        var entries = await session.Reader.GetLinesAsync(
            session.Document,
            startLine,
            count,
            cancellationToken);

        triggers.ScanNewLines(entries);
    }
}
