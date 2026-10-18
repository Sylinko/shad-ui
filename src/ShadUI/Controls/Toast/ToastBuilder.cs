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
    /// <param name="content"></param>
    internal ToastBuilder(ToastHost? host, string title, object? content = null)
    {
        _host = host;
        _toast = Dispatcher.UIThread.Invoke(() => new Toast
        {
            Title = title,
            Content = content
        });
    }

    /// <summary>
    ///     Sets the content of the toast.
    /// </summary>
    /// <param name="content">The content object to display.</param>
    /// <returns>The current <see cref="ToastBuilder" /> instance for fluent chaining.</returns>
    public ToastBuilder WithContent(object content)
    {
        Configure(toast => toast.Content = content);
        return this;
    }

    /// <summary>
    ///     Sets the auto-dismiss delay in seconds.
    /// </summary>
    /// <param name="durationInSeconds">Duration before the toast is automatically dismissed.</param>
    /// <returns>The current <see cref="ToastBuilder" /> instance for fluent chaining.</returns>
    public ToastBuilder WithDurationSeconds(double durationInSeconds)
    {
        Configure(toast => toast.Duration = TimeSpan.FromSeconds(durationInSeconds));
        return this;
    }

    /// <summary>
    ///     Sets the auto-dismiss delay.
    /// </summary>
    /// <param name="duration">Duration before the toast is automatically dismissed.</param>
    /// <returns>The current <see cref="ToastBuilder" /> instance for fluent chaining.</returns>
    public ToastBuilder WithDuration(TimeSpan duration)
    {
        Configure(toast => toast.Duration = duration);
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
        Configure(toast =>
        {
            toast.ActionButtonContent = content;
            toast.ActionButtonStyle = style;
        });
        return this;
    }

    /// <summary>
    ///     Attaches a progress reporter to the toast's progress bar.
    /// </summary>
    /// <param name="progress">The <see cref="Progress{T}" /> instance that reports progress updates.</param>
    /// <returns>The current <see cref="ToastBuilder" /> instance for fluent chaining.</returns>
    public ToastBuilder WithProgress(Progress<double> progress)
    {
        Configure(toast => toast.Progress = progress);
        return this;
    }

    /// <summary>
    ///     Attaches a cancellation token source that will be cancelled when the toast is dismissed.
    /// </summary>
    /// <param name="cancellationTokenSource">The cancellation token source to cancel on dismiss.</param>
    /// <returns>The current <see cref="ToastBuilder" /> instance for fluent chaining.</returns>
    public ToastBuilder WithCancellationTokenSource(CancellationTokenSource cancellationTokenSource)
    {
        Configure(toast => toast.DismissCts = cancellationTokenSource);
        return this;
    }

    /// <summary>
    ///     Closes the toast when the token is cancelled. An already cancelled token prevents
    ///     presentation. Cancellation completes ShowAsync with ToastResult.Dismissed.
    ///     This does not cancel application work unless WithCancellationTokenSource is also used.
    /// </summary>
    public ToastBuilder WithCancellationToken(CancellationToken cancellationToken)
    {
        Configure(toast => toast.CloseToken = cancellationToken);
        return this;
    }

    public ToastBuilder WithPosition(ToastPosition position)
    {
        Configure(toast => toast.Position = position);
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the top left of the screen.
    /// </summary>
    public ToastBuilder OnTopLeft()
    {
        Configure(toast => toast.Position = ToastPosition.TopLeft);
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the top center of the screen.
    /// </summary>
    public ToastBuilder OnTopCenter()
    {
        Configure(toast => toast.Position = ToastPosition.TopCenter);
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the top right of the screen.
    /// </summary>
    public ToastBuilder OnTopRight()
    {
        Configure(toast => toast.Position = ToastPosition.TopRight);
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the bottom left of the screen.
    /// </summary>
    public ToastBuilder OnBottomLeft()
    {
        Configure(toast => toast.Position = ToastPosition.BottomLeft);
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the bottom center of the screen.
    /// </summary>
    public ToastBuilder OnBottomCenter()
    {
        Configure(toast => toast.Position = ToastPosition.BottomCenter);
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the bottom right of the screen.
    /// </summary>
    public ToastBuilder OnBottomRight()
    {
        Configure(toast => toast.Position = ToastPosition.BottomRight);
        return this;
    }

    /// <summary>
    ///     Configures the toast to be dismissed when the user clicks anywhere on the card.
    /// </summary>
    public ToastBuilder DismissOnClick()
    {
        Configure(toast => toast.CanDismissByClicking = true);
        return this;
    }

    /// <summary>
    ///     Shows the toast notification with the specified visual style.
    ///     The host is resolved via <see cref="ToastManager" /> if not explicitly provided.
    ///     Thread-safe: may be called from any thread.
    /// </summary>
    /// <param name="type">The notification style. Defaults to <see cref="Notification.Basic" />.</param>
    /// <returns>The displayed toast. Modify its properties only on the UI thread.</returns>
    public Toast Show(Notification type = Notification.Basic)
    {
        // Capture references to escape the ref struct
        var toast = _toast;
        if (Dispatcher.UIThread.CheckAccess())
        {
            toast.Notification = type;
            QueueToast(_host, toast);
        }
        else
        {
            Dispatcher.UIThread.Post(host =>
            {
                toast.Notification = type;
                QueueToast(host as ToastHost, toast);
            }, _host);
        }

        return toast;
    }

    /// <summary>
    ///     Shows an info-styled toast notification.
    /// </summary>
    public Toast ShowInfo() => Show(Notification.Info);

    /// <summary>
    ///     Shows a success-styled toast notification.
    /// </summary>
    /// <param name="delayInSeconds">Auto-dismiss delay in seconds. Defaults to 5.</param>
    public Toast ShowSuccess(double delayInSeconds = 5d) => WithDurationSeconds(delayInSeconds).Show(Notification.Success);

    /// <summary>
    ///     Shows a warning-styled toast notification.
    /// </summary>
    public Toast ShowWarning() => Show(Notification.Warning);

    /// <summary>
    ///     Shows an error-styled toast notification.
    /// </summary>
    public Toast ShowError() => Show(Notification.Error);

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
        var tcs = new TaskCompletionSource<ToastResult>();
        var toast = _toast;
        toast.CloseRequested += HandleClosed;
        Show(type);

        return tcs.Task;

        void HandleClosed(object? sender, ToastResult result)
        {
            toast.CloseRequested -= HandleClosed;
            tcs.TrySetResult(result);
        }
    }

    private void Configure(Action<Toast> configure)
    {
        var toast = _toast;
        toast.Dispatcher.Invoke(() => configure(toast));
    }

    private static void QueueToast(ToastHost? host, Toast toast)
    {
        if (toast.CloseToken.IsCancellationRequested)
        {
            toast.RequestClose(ToastResult.Dismissed);
            return;
        }

        host ??= ToastManager.ResolveHost();
        if (host is null)
        {
            toast.RequestClose(ToastResult.HostNotFound);
            return;
        }

        host.QueueToast(toast);
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