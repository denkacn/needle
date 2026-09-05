namespace Needle.Avalonia.ViewModels;

using Needle.Avalonia.Services;

internal sealed class UiTaskRunner
{
    private readonly Action<string> _setStatus;

    public UiTaskRunner(Action<string> setStatus)
    {
        _setStatus = setStatus;
    }

    public void Run(Task task, string failureMessage)
    {
        _ = RunCoreAsync(task, failureMessage);
    }

    public void Run(Func<Task> taskFactory, string failureMessage)
    {
        _ = RunCoreAsync(taskFactory, failureMessage);
    }

    private async Task RunCoreAsync(Task task, string failureMessage)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            StartupLogService.WriteException(ex, failureMessage);
            _setStatus($"{failureMessage}: {ex.Message}");
        }
    }

    private async Task RunCoreAsync(Func<Task> taskFactory, string failureMessage)
    {
        try
        {
            await taskFactory();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            StartupLogService.WriteException(ex, failureMessage);
            _setStatus($"{failureMessage}: {ex.Message}");
        }
    }
}
