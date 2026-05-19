using Avalonia.Threading;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     A fluent builder for constructing and queuing <see cref="Toast" /> notifications
///     through the <see cref="ToastManager" /> static router or directly via a
///     <see cref="ToastHost" />.
/// </summary>
public readonly ref struct ToastBuilder
{
    private readonly ToastHost? _host;
    private readonly Toast _toast;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToastBuilder" /> class.
    /// </summary>
    /// <param name="host">
    ///     The <see cref="ToastHost" /> that will display the toast.
    ///     When <c>null</c>, the host is resolved via <see cref="ToastManager.ResolveHost" /> at show time.
    /// </param>
    /// <param name="title">The title text of the toast.</param>
    internal ToastBuilder(ToastHost? host, string title)
    {
        _host = host;
        _toast = new Toast
        {
            Title = title
        };
    }

    /// <summary>
    ///     Sets the content of the toast.
    /// </summary>
    /// <param name="content">The content object to display.</param>
    /// <returns>The current <see cref="ToastBuilder" /> instance for fluent chaining.</returns>
    public ToastBuilder WithContent(object content)
    {
        _toast.Content = content;
        return this;
    }

    /// <summary>
    ///     Sets the auto-dismiss delay in seconds.
    /// </summary>
    /// <param name="durationInSeconds">Duration before the toast is automatically dismissed.</param>
    /// <returns>The current <see cref="ToastBuilder" /> instance for fluent chaining.</returns>
    public ToastBuilder WithDurationSeconds(double durationInSeconds)
    {
        _toast.Duration = TimeSpan.FromSeconds(durationInSeconds);
        return this;
    }

    /// <summary>
    ///     Sets the auto-dismiss delay.
    /// </summary>
    /// <param name="duration">Duration before the toast is automatically dismissed.</param>
    /// <returns>The current <see cref="ToastBuilder" /> instance for fluent chaining.</returns>
    public ToastBuilder WithDuration(TimeSpan duration)
    {
        _toast.Duration = duration;
        return this;
    }

    /// <summary>
    ///     Sets the content and style for the toast's action button.
    /// </summary>
    /// <param name="content">The content displayed on the action button.</param>
    /// <param name="style">The visual style of the action button. Defaults to <see cref="ButtonStyle.Primary" />.</param>
    /// <returns>The current <see cref="ToastBuilder" /> instance for fluent chaining.</returns>
    public ToastBuilder WithAction(object content, ButtonStyle style = ButtonStyle.Primary)
    {
        _toast.ActionButtonContent = content;
        _toast.ActionButtonStyle = style;
        return this;
    }

    /// <summary>
    ///     Attaches a progress reporter to the toast's progress bar.
    /// </summary>
    /// <param name="progress">The <see cref="Progress{T}" /> instance that reports progress updates.</param>
    /// <returns>The current <see cref="ToastBuilder" /> instance for fluent chaining.</returns>
    public ToastBuilder WithProgress(Progress<double> progress)
    {
        _toast.Progress = progress;
        return this;
    }

    /// <summary>
    ///     Attaches a cancellation token source that will be cancelled when the toast is dismissed.
    /// </summary>
    /// <param name="cancellationTokenSource">The cancellation token source to cancel on dismiss.</param>
    /// <returns>The current <see cref="ToastBuilder" /> instance for fluent chaining.</returns>
    public ToastBuilder WithCancellationTokenSource(CancellationTokenSource cancellationTokenSource)
    {
        _toast.DismissCts = cancellationTokenSource;
        return this;
    }

    public ToastBuilder WithPosition(ToastPosition position)
    {
        _toast.Position = position;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the top left of the screen.
    /// </summary>
    public ToastBuilder OnTopLeft()
    {
        _toast.Position = ToastPosition.TopLeft;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the top center of the screen.
    /// </summary>
    public ToastBuilder OnTopCenter()
    {
        _toast.Position = ToastPosition.TopCenter;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the top right of the screen.
    /// </summary>
    public ToastBuilder OnTopRight()
    {
        _toast.Position = ToastPosition.TopRight;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the bottom left of the screen.
    /// </summary>
    public ToastBuilder OnBottomLeft()
    {
        _toast.Position = ToastPosition.BottomLeft;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the bottom center of the screen.
    /// </summary>
    public ToastBuilder OnBottomCenter()
    {
        _toast.Position = ToastPosition.BottomCenter;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the bottom right of the screen.
    /// </summary>
    public ToastBuilder OnBottomRight()
    {
        _toast.Position = ToastPosition.BottomRight;
        return this;
    }

    /// <summary>
    ///     Configures the toast to be dismissed when the user clicks anywhere on the card.
    /// </summary>
    public ToastBuilder DismissOnClick()
    {
        _toast.CanDismissByClicking = true;
        return this;
    }

    /// <summary>
    ///     Shows the toast notification with the specified visual style.
    ///     The host is resolved via <see cref="ToastManager" /> if not explicitly provided.
    ///     Thread-safe: may be called from any thread.
    /// </summary>
    /// <param name="type">The notification style. Defaults to <see cref="Notification.Basic" />.</param>
    public void Show(Notification type = Notification.Basic)
    {
        _toast.Notification = type;

        // Capture references to escape the ref struct
        var toast = _toast;
        if (Dispatcher.UIThread.CheckAccess())
        {
            (_host ?? ToastManager.ResolveHost())?.QueueToast(toast);
        }
        else
        {
            Dispatcher.UIThread.Post(host => (host as ToastHost ?? ToastManager.ResolveHost())?.QueueToast(toast), _host);
        }
    }

    /// <summary>
    ///     Shows an info-styled toast notification.
    /// </summary>
    public void ShowInfo() => Show(Notification.Info);

    /// <summary>
    ///     Shows a success-styled toast notification.
    /// </summary>
    /// <param name="delayInSeconds">Auto-dismiss delay in seconds. Defaults to 5.</param>
    public void ShowSuccess(double delayInSeconds = 5d) => WithDurationSeconds(delayInSeconds).Show(Notification.Success);

    /// <summary>
    ///     Shows a warning-styled toast notification.
    /// </summary>
    public void ShowWarning() => Show(Notification.Warning);

    /// <summary>
    ///     Shows an error-styled toast notification.
    /// </summary>
    public void ShowError() => Show(Notification.Error);

    /// <summary>
    ///     Shows the toast notification asynchronously and returns a task that completes
    ///     with the <see cref="ToastResult" /> when the toast is dismissed.
    ///     The host is resolved via <see cref="ToastManager" /> if not explicitly provided.
    ///     Thread-safe: may be called from any thread.
    /// </summary>
    /// <param name="type">The notification style. Defaults to <see cref="Notification.Basic" />.</param>
    /// <returns>
    ///     A <see cref="Task{ToastResult}" /> that resolves when the toast is dismissed.
    /// </returns>
    public Task<ToastResult> ShowAsync(Notification type = Notification.Basic)
    {
        _toast.Notification = type;

        var tcs = new TaskCompletionSource<ToastResult>();
        _toast.CloseRequested += (_, result) => tcs.TrySetResult(result);

        var toast = _toast;
        if (Dispatcher.UIThread.CheckAccess())
        {
            var host = _host ?? ToastManager.ResolveHost();
            if (host is null)
            {
                tcs.TrySetResult(ToastResult.HostNotFound);
                return tcs.Task;
            }

            host.QueueToast(toast);
        }
        else
        {
            Dispatcher.UIThread.Post(h =>
            {
                var host = h as ToastHost ?? ToastManager.ResolveHost();
                if (host is null)
                {
                    tcs.TrySetResult(ToastResult.HostNotFound);
                    return;
                }

                host.QueueToast(toast);
            }, _host);
        }

        return tcs.Task;
    }

    /// <summary>
    ///     Shows an info-styled toast notification asynchronously.
    /// </summary>
    /// <returns>
    ///     A <see cref="Task{ToastResult}" /> that resolves when the toast is dismissed.
    /// </returns>
    public Task<ToastResult> ShowInfoAsync() => ShowAsync(Notification.Info);

    /// <summary>
    ///     Shows a success-styled toast notification asynchronously.
    /// </summary>
    /// <returns>
    ///     A <see cref="Task{ToastResult}" /> that resolves when the toast is dismissed.
    /// </returns>
    public Task<ToastResult> ShowSuccessAsync() => ShowAsync(Notification.Success);

    /// <summary>
    ///     Shows a warning-styled toast notification asynchronously.
    /// </summary>
    /// <returns>
    ///     A <see cref="Task{ToastResult}" /> that resolves when the toast is dismissed.
    /// </returns>
    public Task<ToastResult> ShowWarningAsync() => ShowAsync(Notification.Warning);

    /// <summary>
    ///     Shows an error-styled toast notification asynchronously.
    /// </summary>
    /// <returns>
    ///     A <see cref="Task{ToastResult}" /> that resolves when the toast is dismissed.
    /// </returns>
    public Task<ToastResult> ShowErrorAsync() => ShowAsync(Notification.Error);
}