namespace Needle.Avalonia.ViewModels;

internal sealed class DebouncedAsyncAction
{
    private readonly UiTaskRunner _tasks;
    private readonly string _failureMessage;
    private readonly TimeSpan _delay;
    private readonly CancellationSlot _cancellation = new();

    public DebouncedAsyncAction(UiTaskRunner tasks, string failureMessage, TimeSpan delay)
    {
        _tasks = tasks;
        _failureMessage = failureMessage;
        _delay = delay;
    }

    public void Queue(Func<CancellationToken, Task> action)
    {
        var cancellationToken = _cancellation.Reset().Token;
        _tasks.Run(() => RunAfterDelayAsync(action, cancellationToken), _failureMessage);
    }

    private async Task RunAfterDelayAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(_delay, cancellationToken);
            await action(cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
        }
    }
}
