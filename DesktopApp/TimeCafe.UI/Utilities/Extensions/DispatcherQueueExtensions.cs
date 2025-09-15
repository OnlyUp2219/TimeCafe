using Microsoft.UI.Dispatching;

namespace TimeCafe.UI.Utilities.Extensions;

public static class DispatcherQueueExtensions
{
    public static Task EnqueueAsync(this DispatcherQueue dispatcher, Action action)
    {
        var taskCompletionSource = new TaskCompletionSource();

        if (!dispatcher.TryEnqueue(() =>
        {
            try
            {
                action();
                taskCompletionSource.SetResult();
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        }))
        {
            taskCompletionSource.SetException(new Exception("Failed to enqueue task"));
        }

        return taskCompletionSource.Task;
    }

    public static Task<T> EnqueueAsync<T>(this DispatcherQueue dispatcher, Func<T> function)
    {
        var taskCompletionSource = new TaskCompletionSource<T>();

        if (!dispatcher.TryEnqueue(() =>
        {
            try
            {
                var result = function();
                taskCompletionSource.SetResult(result);
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        }))
        {
            taskCompletionSource.SetException(new Exception("Failed to enqueue task"));
        }

        return taskCompletionSource.Task;
    }
}