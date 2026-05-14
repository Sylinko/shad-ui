using System.Windows.Input;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

// ReSharper disable once CheckNamespace
namespace ShadUI;

public enum ToastResult
{
    /// <summary>
    ///     The toast was dismissed.
    /// </summary>
    Dismissed,

    /// <summary>
    ///     The toast was dismissed after the timer elapsed.
    /// </summary>
    TimerElapsed,

    /// <summary>
    ///     The toast's action button was clicked.
    /// </summary>
    ActionButtonClicked
}

[TemplatePart("PART_ToastCard", typeof(Border))]
[TemplatePart("PART_ActionButton", typeof(Button))]
[TemplatePart("PART_CloseButton", typeof(Button))]
public class Toast : ContentControl
{
    /// <summary>
    ///     Delay in seconds before the toast is auto dismissed.
    /// </summary>
    public TimeSpan Duration { get; set; }

    public ToastPosition? Position { get; set; }

    /// <summary>
    /// Use lazy initialization for performance optimization.
    /// </summary>
    public TaskCompletionSource<ToastResult> ResultCompletionSource => _resultCompletionSource ??= new TaskCompletionSource<ToastResult>();

    public static readonly StyledProperty<Notification> NotificationProperty =
        AvaloniaProperty.Register<Toast, Notification>(nameof(Notification));

    public Notification Notification
    {
        get => GetValue(NotificationProperty);
        set => SetValue(NotificationProperty, value);
    }

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Toast, string?>(nameof(Title));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<bool> IsEmptyContentProperty =
        AvaloniaProperty.Register<Toast, bool>(nameof(IsEmptyContent), true);

    public bool IsEmptyContent
    {
        get => Content == null;
        private set => SetValue(IsEmptyContentProperty, value);
    }

    public static readonly StyledProperty<double> ProgressValueProperty =
        AvaloniaProperty.Register<Toast, double>(nameof(ProgressValue));

    public double ProgressValue
    {
        get => GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }

    public static readonly DirectProperty<Toast, Progress<double>?> ProgressProperty = AvaloniaProperty.RegisterDirect<Toast, Progress<double>?>(
        nameof(Progress),
        o => o.Progress,
        (o, v) => o.Progress = v);

    public Progress<double>? Progress
    {
        get;
        set
        {
            if (Equals(value, field)) return;

            if (field is not null) field.ProgressChanged -= ProgressChangedHandler;
            SetAndRaise(ProgressProperty, ref field, value);

            if (field is not null) field.ProgressChanged += ProgressChangedHandler;
        }
    }

    public CancellationTokenSource? CancellationTokenSource { get; set; }

    public static readonly StyledProperty<object?> ActionButtonContentProperty =
        AvaloniaProperty.Register<Toast, object?>(nameof(ActionButtonContent));

    public object? ActionButtonContent
    {
        get => GetValue(ActionButtonContentProperty);
        set => SetValue(ActionButtonContentProperty, value);
    }

    public static readonly StyledProperty<ButtonStyle> ActionButtonStyleProperty =
        AvaloniaProperty.Register<Toast, ButtonStyle>(nameof(ActionButtonStyle));

    public ButtonStyle ActionButtonStyle
    {
        get => GetValue(ActionButtonStyleProperty);
        set => SetValue(ActionButtonStyleProperty, value);
    }

    public static readonly StyledProperty<bool> CanDismissByClickingProperty =
        AvaloniaProperty.Register<Toast, bool>(nameof(CanDismissByClicking));

    public bool CanDismissByClicking
    {
        get => GetValue(CanDismissByClickingProperty);
        set => SetValue(CanDismissByClickingProperty, value);
    }

    public static readonly StyledProperty<bool> CanDismissProperty =
        AvaloniaProperty.Register<Toast, bool>(nameof(CanDismiss), true);

    public bool CanDismiss
    {
        get => GetValue(CanDismissProperty);
        set => SetValue(CanDismissProperty, value);
    }

    /// <summary>
    ///     Command to execute when the toast is dismissed, either by timer or user interaction.
    /// </summary>
    public static readonly StyledProperty<ICommand?> DismissCommandProperty =
        AvaloniaProperty.Register<Toast, ICommand?>(nameof(DismissCommand));

    /// <summary>
    ///     Gets or sets the command to execute when the toast is dismissed, either by timer or user interaction.
    /// </summary>
    public ICommand? DismissCommand
    {
        get => GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    private readonly ToastManager? _manager;

    private IDisposable? _dismissTimer;
    private TaskCompletionSource<ToastResult>? _resultCompletionSource;

    public Toast() { }

    public Toast(ToastManager manager)
    {
        _manager = manager;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        e.NameScope.Get<Border>("PART_ToastCard").PointerPressed += ToastCardClickedHandler;
        e.NameScope.Get<Button>("PART_ActionButton").Click += (_, _) =>
        {
            _dismissTimer?.Dispose();
            _dismissTimer = null;
            _resultCompletionSource?.TrySetResult(ToastResult.ActionButtonClicked);

            var duration = TimeSpan.FromMilliseconds(500);
            var easing = new CubicEaseInOut();
            Task.WhenAll(
                    this.Animate(SkewTransform.AngleXProperty).To(10d).WithDuration(duration).WithEasing(easing).RunAsync(),
                    this.Animate(SkewTransform.AngleYProperty).To(10d).WithDuration(duration).WithEasing(easing).RunAsync(),
                    Position switch
                    {
                        ToastPosition.TopLeft or ToastPosition.BottomLeft =>
                            this.Animate(TranslateTransform.XProperty).To(-Bounds.Width - 20d).WithDuration(duration).WithEasing(easing).RunAsync(),
                        ToastPosition.TopRight or ToastPosition.BottomRight =>
                            this.Animate(TranslateTransform.XProperty).To(Bounds.Width + 20d).WithDuration(duration).WithEasing(easing).RunAsync(),
                        ToastPosition.TopCenter =>
                            this.Animate(TranslateTransform.YProperty).To(-Bounds.Height - 20d).WithDuration(duration).WithEasing(easing).RunAsync(),
                        ToastPosition.BottomCenter =>
                            this.Animate(TranslateTransform.YProperty).To(Bounds.Height + 20d).WithDuration(duration).WithEasing(easing).RunAsync(),
                        _ => Task.CompletedTask
                    }
                )
                .ContinueWith(
                    _ => _manager?.Dismiss(this),
                    TaskScheduler.FromCurrentSynchronizationContext());
        };
        e.NameScope.Get<Button>("PART_CloseButton").Click += (_, _) => _manager?.Dismiss(this);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        _dismissTimer?.Dispose();
        _dismissTimer = null;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        StartDismissTimer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _dismissTimer?.Dispose();
        _dismissTimer = null;
        _resultCompletionSource?.TrySetResult(ToastResult.Dismissed);
    }

    private void ProgressChangedHandler(object? sender, double e)
    {
        var dispatcher = Dispatcher.UIThread;
        if (dispatcher.CheckAccess())
        {
            ProgressValue = e;
        }
        else
        {
            dispatcher.Post(() => ProgressValue = e);
        }
    }

    private void ToastCardClickedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (!CanDismissByClicking) return;
        _manager?.Dismiss(this);
    }

    public void Show()
    {
        this.Animate(OpacityProperty)
            .From(0d)
            .To(1d)
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithEasing(new CubicEaseInOut())
            .Start();

        this.Animate(MaxHeightProperty)
            .From(0)
            .To(500)
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithEasing(new CubicEaseInOut())
            .Start();

        this.Animate(MarginProperty)
            .From(new Thickness(0, 10, 0, -10))
            .To(new Thickness())
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithEasing(new CubicEaseInOut())
            .Start();

        StartDismissTimer();
    }

    private void StartDismissTimer()
    {
        if (Duration.Ticks <= 0) return;

        _dismissTimer ??= DispatcherTimer.RunOnce(
            () =>
            {
                _resultCompletionSource?.TrySetResult(ToastResult.TimerElapsed);
                _manager?.Dismiss(this);
            },
            Duration);
    }

    public void Dismiss()
    {
        CancellationTokenSource?.Cancel();

        this.Animate(OpacityProperty)
            .From(1d)
            .To(0d)
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithEasing(new CubicEaseInOut())
            .Start();

        this.Animate(MarginProperty)
            .From(new Thickness())
            .To(new Thickness(0, 0, 0, -100))
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithEasing(new CubicEaseInOut())
            .Start();

        if (DismissCommand is { } dismissCommand && dismissCommand.CanExecute(null)) dismissCommand.Execute(null);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty) IsEmptyContent = Content == null;
    }
}