using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Data;
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

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() => frame.Continue = false);
        }

        task.ContinueWith(
            t =>
            {
                capturedException = t.Exception;
                frame.Continue = false;
            },
            TaskContinuationOptions.AttachedToParent);

        Dispatcher.UIThread.PushFrame(frame); // This will wait until `frame.Continue = false`

        if (capturedException != null)
        {
            throw capturedException;
        }
    }

    /// <summary>
    /// Run and wait for a Task on the DispatcherFrame, allowing the UI thread to remain responsive
    /// </summary>
    /// <param name="task"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="AggregateException"></exception>
    public static TResult WaitOnDispatcherFrame<TResult>(this Task<TResult> task, CancellationToken cancellationToken = default)
    {
        var frame = new DispatcherFrame();

        TResult result = default!;
        AggregateException? capturedException = null;

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() => frame.Continue = false);
        }

        task.ContinueWith(
            t =>
            {
                capturedException = t.Exception;
                result = t.Result;
                frame.Continue = false;
            },
            TaskContinuationOptions.AttachedToParent);

        Dispatcher.UIThread.PushFrame(frame); // This will wait until `frame.Continue = false`

        if (capturedException != null)
        {
            throw capturedException;
        }

        return result;
    }

    extension(AvaloniaProperty avaloniaProperty)
    {
        public void ForceOverrideMetadata(Type type, AvaloniaPropertyMetadata metadata)
        {
            var metadataDictionary = avaloniaProperty.GetMetadataUnsafe();
            var baseMetadata = metadataDictionary.GetValueOrDefault(type, avaloniaProperty.GetMetadata(type));
            metadata.Merge(baseMetadata, avaloniaProperty);
            metadata.Freeze();

            metadataDictionary[type] = metadata;
            avaloniaProperty.GetMetadataCacheUnsafe().Clear();
            avaloniaProperty.GetSingleMetadataUnsafe() = null;
            avaloniaProperty.GetSingleHostTypeUnsafe() = null;
        }

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_metadata")]
        private extern ref Dictionary<Type, AvaloniaPropertyMetadata> GetMetadataUnsafe();

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_metadataCache")]
        private extern ref Dictionary<Type, AvaloniaPropertyMetadata> GetMetadataCacheUnsafe();

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_singleMetadata")]
        private extern ref AvaloniaPropertyMetadata? GetSingleMetadataUnsafe();

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_singleHostType")]
        private extern ref Type? GetSingleHostTypeUnsafe();
    }

    extension<TValue>(StyledProperty<TValue> styledProperty)
    {
        public void ForceOverrideMetadata(Type type, StyledPropertyMetadata<TValue> metadata)
        {
            ((AvaloniaProperty)styledProperty).ForceOverrideMetadata(type, metadata);

            ref var singleDefaultValue = ref UnsafeAccessor<TValue>.GetSingleDefaultValueUnsafe(styledProperty);
            if (singleDefaultValue != metadata.DefaultValue)
            {
                singleDefaultValue = default;
            }
        }

        public void ForceOverrideDefaultValue(Type type, TValue defaultValue)
        {
            styledProperty.ForceOverrideMetadata(type, new StyledPropertyMetadata<TValue>(defaultValue));
        }
    }

    private static class UnsafeAccessor<TValue>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_singleDefaultValue")]
        public static extern ref Optional<TValue> GetSingleDefaultValueUnsafe(StyledProperty<TValue> styledProperty);
    }
}