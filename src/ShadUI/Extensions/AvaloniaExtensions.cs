using Avalonia.Threading;

namespace ShadUI.Extensions;

public static class AvaloniaExtensions
{
    extension(Dispatcher dispatcher)
    {
        public Task InvokeOnDemandAsync(Action action)
        {
            if (!dispatcher.CheckAccess()) return dispatcher.InvokeAsync(action).GetTask();
            action();
            return Task.CompletedTask;
        }

        public Task InvokeOnDemandAsync(
            Action action,
            in DispatcherPriority priority)
        {
            if (!dispatcher.CheckAccess()) return dispatcher.InvokeAsync(action, priority).GetTask();
            action();
            return Task.CompletedTask;
        }

        public Task InvokeOnDemandAsync(
            Action action,
            in DispatcherPriority priority,
            in CancellationToken cancellationToken)
        {
            if (!dispatcher.CheckAccess()) return dispatcher.InvokeAsync(action, priority, cancellationToken).GetTask();
            action();
            return Task.CompletedTask;
        }

        public Task<T> InvokeOnDemandAsync<T>(Func<T> func)
        {
            return dispatcher.CheckAccess() ? Task.FromResult(func()) : dispatcher.InvokeAsync(func).GetTask();
        }

        public Task<T> InvokeOnDemandAsync<T>(
            Func<T> func,
            in DispatcherPriority priority)
        {
            return dispatcher.CheckAccess() ? Task.FromResult(func()) : dispatcher.InvokeAsync(func, priority).GetTask();
        }

        public Task<T> InvokeOnDemandAsync<T>(
            Func<T> func,
            in DispatcherPriority priority,
            in CancellationToken cancellationToken)
        {
            return dispatcher.CheckAccess() ? Task.FromResult(func()) : dispatcher.InvokeAsync(func, priority, cancellationToken).GetTask();
        }

        public void PostOnDemand(Action action, DispatcherPriority priority = default)
        {
            if (dispatcher.CheckAccess()) action();
            else dispatcher.Post(action, priority);
        }
    }

    /// <summary>
    /// Run and wait for a Task on the DispatcherFrame, allowing the UI thread to remain responsive
    /// </summary>
    /// <param name="task"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="AggregateException"></exception>
    public static void WaitOnDispatcherFrame(this Task task, CancellationToken cancellationToken = default)
    {
        var frame = new DispatcherFrame();
        AggregateException? capturedException = null;

        if (cancellationToken != CancellationToken.None)
        {
            cancellationToken.Register(() => frame.Continue = false);
        }
        task.ContinueWith(
            t =>
            {
                capturedException = t.Exception;
                frame.Continue = false; // 结束消息循环
            },
            TaskContinuationOptions.AttachedToParent);

        Dispatcher.UIThread.PushFrame(frame);

        if (capturedException != null)
        {
            throw capturedException;
        }
    }

    /// <summary>
    /// Run and wait for a Task on the DispatcherFrame, allowing the UI thread to remain responsive
    /// </summary>
    /// <param name="task"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="AggregateException"></exception>
    public static TResult WaitOnDispatcherFrame<TResult>(this Task<TResult> task)
    {
        var frame = new DispatcherFrame();

        TResult? result = default;

        AggregateException? capturedException = null;

        task.ContinueWith(
            t =>
            {
                capturedException = t.Exception;
                result = t.Result;
                frame.Continue = false; // 结束消息循环
            },
            TaskContinuationOptions.AttachedToParent);

        Dispatcher.UIThread.PushFrame(frame);

        if (capturedException != null)
        {
            throw capturedException;
        }

        return result ?? throw new InvalidOperationException("Task result is null");
    }
}