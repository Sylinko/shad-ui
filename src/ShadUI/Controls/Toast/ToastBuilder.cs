

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Builds a toast notification.
/// </summary>
public sealed class ToastBuilder
{
    private readonly ToastManager _manager;
    private readonly Toast _toast;

    /// <summary>
    ///     Returns a new instance of <see cref="ToastBuilder" />.
    /// </summary>
    internal ToastBuilder(ToastManager manager, string title)
    {
        _manager = manager;
        _toast = new Toast(_manager)
        {
            Title = title
        };
    }

    /// <summary>
    ///     Sets the content of the toast.
    /// </summary>
    public ToastBuilder WithContent(object content)
    {
        _toast.Content = content;
        return this;
    }

    /// <summary>
    ///     Sets the delay before the toast is dismissed in seconds.
    /// </summary>
    public ToastBuilder WithDurationSeconds(double durationInSeconds)
    {
        _toast.Duration = TimeSpan.FromSeconds(durationInSeconds);
        return this;
    }

    /// <summary>
    ///     Sets the delay before the toast is dismissed in seconds.
    /// </summary>
    public ToastBuilder WithDuration(TimeSpan duration)
    {
        _toast.Duration = duration;
        return this;
    }

    /// <summary>
    ///     Sets the action callback and label for the toast's action button.
    /// </summary>
    public ToastBuilder WithAction(object content, ButtonStyle style = ButtonStyle.Primary)
    {
        _toast.ActionButtonContent = content;
        _toast.ActionButtonStyle = style;
        return this;
    }

    /// <summary>
    ///     Sets the progress bar for the toast notification.
    /// </summary>
    public ToastBuilder WithProgress(Progress<double> progress)
    {
        _toast.Progress = progress;
        return this;
    }

    /// <summary>
    ///     Sets the cancellation token source for the toast notification. It will be canceled when toast is dismissed.
    /// </summary>
    public ToastBuilder WithCancellationTokenSource(CancellationTokenSource cancellationTokenSource)
    {
        _toast.CancellationTokenSource = cancellationTokenSource;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the top left.
    /// </summary>
    public ToastBuilder OnTopLeft()
    {
        _toast.Position = ToastPosition.TopLeft;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the top center.
    /// </summary>
    public ToastBuilder OnTopCenter()
    {
        _toast.Position = ToastPosition.TopCenter;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the top right.
    /// </summary>
    public ToastBuilder OnTopRight()
    {
        _toast.Position = ToastPosition.TopRight;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the bottom left.
    /// </summary>
    public ToastBuilder OnBottomLeft()
    {
        _toast.Position = ToastPosition.BottomLeft;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the bottom center.
    /// </summary>
    public ToastBuilder OnBottomCenter()
    {
        _toast.Position = ToastPosition.BottomCenter;
        return this;
    }

    /// <summary>
    ///     Sets the toast position to the bottom right.
    /// </summary>
    public ToastBuilder OnBottomRight()
    {
        _toast.Position = ToastPosition.BottomRight;
        return this;
    }

    /// <summary>
    ///     Sets the toast to be dismissed when clicked.
    /// </summary>
    public ToastBuilder DismissOnClick()
    {
        _toast.CanDismissByClicking = true;
        return this;
    }

    /// <summary>
    ///     Shows the toast notification with the specified type. The default is <see cref="Notification.Basic" />.
    /// </summary>
    /// <param name="type"></param>
    public void Show(Notification type = Notification.Basic)
    {
        _toast.Notification = type;
        _manager.Queue(_toast);
    }

    /// <summary>
    ///     Shows an info styled toast notification
    /// </summary>
    public void ShowInfo() => Show(Notification.Info);

    /// <summary>
    ///     Shows a success styled toast notification
    /// </summary>
    public void ShowSuccess(double delayInSeconds = 5d) => WithDurationSeconds(delayInSeconds).Show(Notification.Success);

    /// <summary>
    ///     Shows a warning styled toast notification
    /// </summary>
    public void ShowWarning() => Show(Notification.Warning);

    /// <summary>
    ///     Shows an error styled toast notification
    /// </summary>
    public void ShowError() => Show(Notification.Error);

    /// <summary>
    ///     Shows a toast notification in the specified style.
    /// </summary>
    public Task<ToastResult> ShowAsync(Notification type = Notification.Basic)
    {
        _toast.Notification = type;
        _manager.Queue(_toast);

        return _toast.ResultCompletionSource.Task;
    }

    /// <summary>
    ///     Shows an info styled toast notification
    /// </summary>
    /// <returns></returns>
    public Task<ToastResult> ShowInfoAsync() => ShowAsync(Notification.Info);

    /// <summary>
    ///     Shows a success styled toast notification
    /// </summary>
    /// <returns></returns>
    public Task<ToastResult> ShowSuccessAsync() => ShowAsync(Notification.Success);

    /// <summary>
    ///     Shows a warning styled toast notification
    /// </summary>
    /// <returns></returns>
    public Task<ToastResult> ShowWarningAsync() => ShowAsync(Notification.Warning);

    /// <summary>
    ///     Shows an error styled toast notification
    /// </summary>
    /// <returns></returns>
    public Task<ToastResult> ShowErrorAsync() => ShowAsync(Notification.Error);
}