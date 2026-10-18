using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Hosts and manages the lifecycle of <see cref="Toast" /> notifications.
///     Contains four corner-aligned <see cref="ToastCornerHost" /> instances;
///     toasts are routed to the correct corner based on <see cref="Toast.Position" />.
/// </summary>
public sealed class ToastHost : TemplatedControl
{
    /// <summary>
    ///     Defines the <see cref="MaxToasts" /> property.
    /// </summary>
    public static readonly StyledProperty<byte> MaxToastsProperty =
        AvaloniaProperty.Register<ToastHost, byte>(nameof(MaxToasts), 5);

    /// <summary>
    ///     Gets or sets the maximum number of toasts displayed simultaneously.
    ///     When exceeded, the oldest toasts are dismissed.
    /// </summary>
    public byte MaxToasts
    {
        get => GetValue(MaxToastsProperty);
        set => SetValue(MaxToastsProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="SingleToast" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> SingleToastProperty =
        AvaloniaProperty.Register<ToastHost, bool>(nameof(SingleToast));

    /// <summary>
    ///     Gets or sets a value indicating whether only a single toast is displayed at a time.
    ///     When <c>true</c>, queuing a new toast dismisses all existing toasts first.
    /// </summary>
    public bool SingleToast
    {
        get => GetValue(SingleToastProperty);
        set => SetValue(SingleToastProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="ToastMaxWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ToastMaxWidthProperty =
        AvaloniaProperty.Register<ToastHost, double>(nameof(ToastMaxWidth), 800);

    /// <summary>
    ///     Gets or sets the maximum width applied to each toast.
    /// </summary>
    public double ToastMaxWidth
    {
        get => GetValue(ToastMaxWidthProperty);
        set => SetValue(ToastMaxWidthProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="ToastMaxHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ToastMaxHeightProperty =
        AvaloniaProperty.Register<ToastHost, double>(nameof(ToastMaxHeight), double.PositiveInfinity);

    /// <summary>
    ///     Gets or sets the maximum height applied to each toast.
    /// </summary>
    public double ToastMaxHeight
    {
        get => GetValue(ToastMaxHeightProperty);
        set => SetValue(ToastMaxHeightProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="ToastMinWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ToastMinWidthProperty =
        AvaloniaProperty.Register<ToastHost, double>(nameof(ToastMinWidth), 320);

    /// <summary>
    ///     Gets or sets the minimum width applied to each toast.
    /// </summary>
    public double ToastMinWidth
    {
        get => GetValue(ToastMinWidthProperty);
        set => SetValue(ToastMinWidthProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="ToastMinHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ToastMinHeightProperty =
        AvaloniaProperty.Register<ToastHost, double>(nameof(ToastMinHeight));

    /// <summary>
    ///     Gets or sets the minimum height applied to each toast.
    /// </summary>
    public double ToastMinHeight
    {
        get => GetValue(ToastMinHeightProperty);
        set => SetValue(ToastMinHeightProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Spacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<ToastHost, double>(nameof(Spacing));

    /// <summary>
    ///     Gets or sets the spacing between toasts in the same corner.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    ///     Default animation duration used for show / hide transitions.
    /// </summary>
    private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(500);

    private ToastCornerHost? _topLeft;
    private ToastCornerHost? _topRight;
    private ToastCornerHost? _bottomLeft;
    private ToastCornerHost? _bottomRight;
    private readonly List<Toast> _queuedToasts = [];
    private readonly HashSet<Toast> _subscribedToasts = [];
    private readonly HashSet<Toast> _shownToasts = [];

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _topLeft = e.NameScope.Get<ToastCornerHost>("PART_TopLeft");
        _topRight = e.NameScope.Get<ToastCornerHost>("PART_TopRight");
        _bottomLeft = e.NameScope.Get<ToastCornerHost>("PART_BottomLeft");
        _bottomRight = e.NameScope.Get<ToastCornerHost>("PART_BottomRight");

        // Toasts may be queued before the host template has been applied. Present them now that
        // the corner hosts exist, and re-present them if a template rebuild replaced the corners.
        foreach (var toast in _queuedToasts.ToArray())
        {
            ShowToastInternal(toast);
        }

        EnsureMaximum(MaxToasts);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ToastManager.Register(this);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ToastManager.Unregister(this);
    }

    #region Public API

    /// <summary>
    ///     Creates a <see cref="ToastBuilder" /> for building and showing a toast
    ///     through this host.
    /// </summary>
    /// <param name="title">The title of the toast.</param>
    /// <param name="content">The content of the toast.</param>
    /// <returns>A fluent <see cref="ToastBuilder" /> instance.</returns>
    public ToastBuilder CreateToast(string title, object? content = null) => new(this, title, content);

    /// <summary>
    ///     Queues a toast for display. Routes to the correct corner based on
    ///     <see cref="Toast.Position" /> (defaults to <see cref="ToastPosition.BottomRight" />).
    /// </summary>
    /// <param name="toast">The toast to display.</param>
    public void QueueToast(Toast toast)
    {
        if (MaxToasts <= 0) return;

        if (SingleToast) DismissAll();

        _queuedToasts.Add(toast);
        ShowToastInternal(toast);
        EnsureMaximum(MaxToasts);
    }

    /// <summary>
    ///     Dismisses a toast with the specified result and plays the exit animation.
    /// </summary>
    /// <param name="toast">The toast to dismiss.</param>
    /// <param name="result">The reason for dismissal.</param>
    public void DismissToast(Toast toast, ToastResult result)
    {
        if (!_queuedToasts.Contains(toast)) return;

        _queuedToasts.Remove(toast);
        CancelAutoDismissTimer(toast);
        CancelDismissCts(toast);
        toast.DismissCts = null;

        AnimateDismiss(toast, result, () =>
        {
            // Remove from whichever corner host it's in
            foreach (var corner in GetAllCornerHosts())
                corner.Items.Remove(toast);

            CleanupToast(toast);
        });
    }

    /// <summary>
    ///     Dismisses all currently displayed toasts on all corners.
    /// </summary>
    public void DismissAll()
    {
        foreach (var toast in _queuedToasts.ToArray())
            DismissToast(toast, ToastResult.Dismissed);
    }

    #endregion

    /// <summary>
    ///     Ensures the number of toasts does not exceed <paramref name="maxAllowed" /> by dismissing
    ///     the oldest evictable toast. Toasts marked <see cref="Toast.CanBeEvicted" /> as
    ///     <c>false</c> are never removed by capacity management; if every remaining toast is
    ///     protected, the host temporarily exceeds the limit rather than dismissing active work.
    /// </summary>
    private void EnsureMaximum(int maxAllowed)
    {
        while (_queuedToasts.Count > maxAllowed)
        {
            Toast? oldestEvictable = null;
            foreach (var toast in _queuedToasts)
            {
                if (!toast.CanBeEvicted) continue;
                oldestEvictable = toast;
                break;
            }

            if (oldestEvictable is null) return;

            DismissToast(oldestEvictable, ToastResult.Dismissed);
        }
    }

    private ToastCornerHost? GetCornerHost(ToastPosition position)
    {
        return position switch
        {
            ToastPosition.TopLeft => _topLeft,
            ToastPosition.TopRight => _topRight,
            ToastPosition.BottomLeft => _bottomLeft,
            _ => _bottomRight
        };
    }

    private IEnumerable<ToastCornerHost> GetAllCornerHosts()
    {
        if (_topLeft is not null) yield return _topLeft;
        if (_topRight is not null) yield return _topRight;
        if (_bottomLeft is not null) yield return _bottomLeft;
        if (_bottomRight is not null) yield return _bottomRight;
    }

    private void ShowToastInternal(Toast toast)
    {
        var corner = GetCornerHost(toast.Position ?? ToastPosition.BottomRight);
        if (corner is null) return;

        if (!corner.Items.Contains(toast))
        {
            corner.Items.Add(toast);
        }

        if (_subscribedToasts.Add(toast))
        {
            toast.CloseRequested += HandleToastCloseRequested;
            toast.ActionButtonClicked += HandleToastActionButtonClicked;
            toast.PointerEntered += HandleToastPointerEntered;
            toast.PointerExited += HandleToastPointerExited;
        }

        if (!_shownToasts.Add(toast)) return;

        AnimateShow(toast);
        StartAutoDismissTimer(toast);
    }

    private void HandleToastCloseRequested(object? sender, ToastResult result)
    {
        if (sender is Toast toast)
        {
            DismissToast(toast, result);
        }
    }

    private void HandleToastActionButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Toast toast && toast.CanDismissOnAction && _queuedToasts.Contains(toast))
        {
            DismissToast(toast, ToastResult.ActionButtonClicked);
        }
    }

    private static void HandleToastPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Toast toast)
            CancelAutoDismissTimer(toast);
    }

    private void HandleToastPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Toast toast)
            StartAutoDismissTimer(toast);
    }

    private void CleanupToast(Toast toast)
    {
        toast.CloseRequested -= HandleToastCloseRequested;
        toast.ActionButtonClicked -= HandleToastActionButtonClicked;
        toast.PointerEntered -= HandleToastPointerEntered;
        toast.PointerExited -= HandleToastPointerExited;

        // Release the caller's progress source so a completed operation does not keep receiving
        // UI-thread progress notifications, and clear timers owned by this host.
        toast.Progress = null;
        CancelAutoDismissTimer(toast);
        CancelDismissCts(toast);

        _subscribedToasts.Remove(toast);
        _shownToasts.Remove(toast);
    }

    private static void AnimateShow(Toast toast)
    {
        toast.Animate(OpacityProperty)
            .From(0d)
            .To(1d)
            .WithDuration(AnimationDuration)
            .WithEasing(new CubicEaseInOut())
            .Start();
    }

    private static void AnimateDismiss(Toast toast, ToastResult result, Action onCompleted)
    {
        // Notify the toast so it can execute its own Command
        toast.RequestClose(result);

        toast.Animate(OpacityProperty)
            .From(1d)
            .To(0d)
            .WithDuration(AnimationDuration)
            .WithEasing(new CubicEaseInOut())
            .Start();

        Task.Delay(AnimationDuration)
            .ContinueWith(_ => onCompleted(), TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void StartAutoDismissTimer(Toast toast)
    {
        if (toast.Duration.Ticks <= 0) return;

        CancelAutoDismissTimer(toast);

        toast.AutoDismissCts = new CancellationTokenSource();
        var cts = toast.AutoDismissCts;

        DispatcherTimer.RunOnce(
            () =>
            {
                if (cts.IsCancellationRequested) return;
                DismissToast(toast, ToastResult.TimerElapsed);
            },
            toast.Duration);
    }

    internal static void CancelAutoDismissTimer(Toast toast)
    {
        toast.AutoDismissCts?.Cancel();
        toast.AutoDismissCts = null;
    }

    private static void CancelDismissCts(Toast toast)
    {
        try
        {
            toast.DismissCts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The caller may own and dispose the source after the associated work ends.
        }
        finally
        {
            toast.DismissCts = null;
        }
    }
}

/// <summary>
///     Corner-specific <see cref="ItemsControl" /> that binds
///     <see cref="ToastHost.ToastMaxWidth" /> and <see cref="ToastHost.ToastMaxHeight" />
///     onto each toast's container.
/// </summary>
internal sealed class ToastCornerHost : ItemsControl
{
    protected override Type StyleKeyOverride => typeof(ItemsControl);

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return item as Toast ?? new ContentControl();
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (item is not Toast toast) return;

        // Walk up to the owning ToastHost via visual tree
        var host = this.FindAncestorOfType<ToastHost>();
        if (host is null) return;

        toast[!MaxWidthProperty] = host[!ToastHost.ToastMaxWidthProperty];
        toast[!MaxHeightProperty] = host[!ToastHost.ToastMaxHeightProperty];
        toast[!MinWidthProperty] = host[!ToastHost.ToastMinWidthProperty];
        toast[!MinHeightProperty] = host[!ToastHost.ToastMinHeightProperty];
    }
}
