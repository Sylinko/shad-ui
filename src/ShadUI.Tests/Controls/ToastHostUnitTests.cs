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
}
