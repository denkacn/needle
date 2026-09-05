namespace Needle.Avalonia.ViewModels;

internal sealed class CancellationSlot
{
    private CancellationTokenSource? _source;

    public CancellationTokenSource Reset()
    {
        CancelAndDispose();
        _source = new CancellationTokenSource();
        return _source;
    }

    public void CancelAndDispose()
    {
        var current = _source;
        _source = null;
        if (current is null)
        {
            return;
        }

        try
        {
            current.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        current.Dispose();
    }
}
