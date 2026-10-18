using System.Reflection;
using Avalonia.Threading;
using Xunit;

namespace ShadUI.Tests.Controls;

public sealed class ToastHostUnitTests
{
    public ToastHostUnitTests()
    {
        AvaloniaTestFixture.EnsureInitialized();
    }

    [Fact]
    public void CancelAutoDismissTimer_Should_Not_Cancel_DismissCts()
    {
        using var dismissCts = new CancellationTokenSource();
        using var autoDismissCts = new CancellationTokenSource();

        var toast = new Toast
        {
            DismissCts = dismissCts,
            AutoDismissCts = autoDismissCts
        };

        ToastHost.CancelAutoDismissTimer(toast);

        Assert.True(autoDismissCts.IsCancellationRequested);
        Assert.Null(toast.AutoDismissCts);
        Assert.False(dismissCts.IsCancellationRequested);
    }

    [Fact]
    public void DismissToast_Should_Cancel_DismissCts()
    {
        using var dismissCts = new CancellationTokenSource();

        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var host = new ToastHost();
            var toast = new Toast
            {
                DismissCts = dismissCts
            };

            host.QueueToast(toast);
            host.DismissToast(toast, ToastResult.Dismissed);
        });

        Assert.True(dismissCts.IsCancellationRequested);
    }

    [Fact]
    public void Show_WhenReturnedToastIsModified_UpdatesTheQueuedInstance()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var host = new ToastHost();
            var toast = host.CreateToast("Before").ShowInfo();
            toast.Title = "After";
            toast.Content = "Changed";

            Assert.Same(toast, Assert.Single(GetQueuedToasts(host)));
            Assert.Equal("After", toast.Title);
            Assert.Equal("Changed", toast.Content);
            host.DismissToast(toast, ToastResult.Dismissed);
        });
    }

    [Fact]
    public void Cancellation_WhenTokenIsAlreadyCancelled_DoesNotQueueToast()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var host = new ToastHost();
            using var lifetime = new CancellationTokenSource();
            lifetime.Cancel();
            var result = host.CreateToast("Hidden").WithCancellationToken(lifetime.Token).ShowInfoAsync();

            Assert.True(result.IsCompletedSuccessfully);
            Assert.Equal(ToastResult.Dismissed, GetCompletedResult(result));
            Assert.Empty(GetQueuedToasts(host));
        });
    }

    [Fact]
    public void Cancellation_FromBackgroundThread_ClosesOnceAndCompletesShowAsync()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var host = new ToastHost();
            using var lifetime = new CancellationTokenSource();
            var result = host.CreateToast("Working").WithCancellationToken(lifetime.Token).ShowInfoAsync();
            var toast = Assert.Single(GetQueuedToasts(host));
            var closeCount = 0;
            toast.CloseRequested += (_, _) => closeCount++;

            var thread = new Thread(() => lifetime.Cancel());
            thread.Start();
            Assert.True(thread.Join(TimeSpan.FromSeconds(5)));
            Dispatcher.UIThread.RunJobs();
            host.DismissToast(toast, ToastResult.Dismissed);

            Assert.Equal(1, closeCount);
            Assert.Empty(GetQueuedToasts(host));
            Assert.True(result.IsCompletedSuccessfully);
            Assert.Equal(ToastResult.Dismissed, GetCompletedResult(result));
            Assert.False(toast.CloseRegistration.Unregister());
        });
    }

    [Fact]
    public void Dismissal_BeforeTokenCancellation_ReleasesRegistrationAndPreservesResult()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var host = new ToastHost();
            using var lifetime = new CancellationTokenSource();
            using var work = new CancellationTokenSource();
            var result = host.CreateToast("Working")
                .WithCancellationToken(lifetime.Token)
                .WithCancellationTokenSource(work)
                .ShowInfoAsync();
            var toast = Assert.Single(GetQueuedToasts(host));
            var closeCount = 0;
            toast.CloseRequested += (_, _) => closeCount++;

            host.DismissToast(toast, ToastResult.TimerElapsed);
            Assert.True(work.IsCancellationRequested);
            Assert.False(toast.CloseRegistration.Unregister());
            lifetime.Cancel();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, closeCount);
            Assert.Equal(ToastResult.TimerElapsed, GetCompletedResult(result));
        });
    }

    [Fact]
    public void Cancellation_BeforeDispatcherQueuesToast_DoesNotPresentIt()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var host = new ToastHost();
            using var lifetime = new CancellationTokenSource();
            var showTask = Task.Run(() =>
            {
                var builder = host.CreateToast("Pending").WithCancellationToken(lifetime.Token);
                lifetime.Cancel();
                return builder.ShowInfo();
            });
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (!showTask.IsCompleted && DateTime.UtcNow < deadline)
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(1);
            }
            Assert.True(showTask.IsCompleted);
            var toast = GetCompletedResult(showTask);
            Dispatcher.UIThread.RunJobs();

            Assert.Empty(GetQueuedToasts(host));
            Assert.Equal("Pending", toast.Title);
        });
    }

    [Fact]
    public void ShowAsync_WhenNoHostExists_CompletesWithoutRetainingCancellation()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            using var lifetime = new CancellationTokenSource();
            var result = ToastManager.Create("No host").WithCancellationToken(lifetime.Token).ShowInfoAsync();
            Assert.True(result.IsCompletedSuccessfully);
            Assert.Equal(ToastResult.HostNotFound, GetCompletedResult(result));
            lifetime.Cancel();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(ToastResult.HostNotFound, GetCompletedResult(result));
        });
    }

    private static T GetCompletedResult<T>(Task<T> task)
    {
        Assert.True(task.IsCompleted);
        return task.GetAwaiter().GetResult();
    }

    private static List<Toast> GetQueuedToasts(ToastHost host)
    {
        var field = typeof(ToastHost).GetField("_queuedToasts", BindingFlags.Instance | BindingFlags.NonPublic);
        return Assert.IsType<List<Toast>>(field?.GetValue(host));
    }
}
