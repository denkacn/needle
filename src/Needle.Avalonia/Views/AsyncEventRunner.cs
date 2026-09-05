using System.Diagnostics;
using Needle.Avalonia.Services;

namespace Needle.Avalonia.Views;

internal static class AsyncEventRunner
{
    public static void Run(Func<Task> action)
    {
        _ = RunCoreAsync(action);
    }

    private static async Task RunCoreAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            StartupLogService.WriteException(ex, "Async event failed");
        }
    }
}
