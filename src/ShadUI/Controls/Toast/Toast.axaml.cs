using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ShadUI.Extensions;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     A pure toast notification control with no built-in lifecycle management.
///     Use standalone in any layout by binding <see cref="Command" />,
///     or let <see cref="ToastHost" /> manage its show / hide animations, auto-dismiss timer, and stacking.
/// </summary>
[TemplatePart("PART_ToastCard", typeof(Border))]
[TemplatePart("PART_ActionButton", typeof(Button))]
[TemplatePart("PART_CloseButton", typeof(Button))]
public sealed class Toast : ContentControl
{

    #region Styled Properties

    /// <summary>
    ///     Defines the <see cref="Notification" /> property.
    /// </summary>
    public static readonly StyledProperty<Notification> NotificationProperty =
        AvaloniaProperty.Register<Toast, Notification>(nameof(Notification));

    /// <summary>
    ///     Gets or sets the visual style of the toast notification.
    /// </summary>
    public Notification Notification
    {
        get => GetValue(NotificationProperty);
        set => SetValue(NotificationProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Toast, string?>(nameof(Title));

    /// <summary>
    ///     Gets or sets the title text displayed on the toast.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="IsEmptyContent" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsEmptyContentProperty =
        AvaloniaProperty.Register<Toast, bool>(nameof(IsEmptyContent), true);

    /// <summary>
    ///     Gets a value indicating whether the <see cref="ContentControl.Content" /> is null.
    /// </summary>
    public bool IsEmptyContent
    {
        get => Content == null;
        private set => SetValue(IsEmptyContentProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="ProgressValue" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ProgressValueProperty =
        AvaloniaProperty.Register<Toast, double>(nameof(ProgressValue));

    /// <summary>
    ///     Gets or sets the current progress value (0.0 to 1.0) displayed on the progress bar.
    /// </summary>
    public double ProgressValue
    {
        get => GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Progress" /> direct property.
    /// </summary>
    public static readonly DirectProperty<Toast, Progress<double>?> ProgressProperty =
        AvaloniaProperty.RegisterDirect<Toast, Progress<double>?>(
            nameof(Progress),
            o => o.Progress,
            (o, v) => o.Progress = v);

    /// <summary>
    ///     Gets or sets an <see cref="System.Progress{T}" /> instance that drives
    ///     <see cref="ProgressValue" />. When set, the control subscribes to progress
    ///     change notifications and updates the UI on the dispatcher thread.
    /// </summary>
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

    /// <summary>
    ///     Defines the <see cref="ActionButtonContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ActionButtonContentProperty =
        AvaloniaProperty.Register<Toast, object?>(nameof(ActionButtonContent));

    /// <summary>
    ///     Gets or sets the content displayed on the action button.
    ///     When null, the action button is hidden.
    /// </summary>
    public object? ActionButtonContent
    {
        get => GetValue(ActionButtonContentProperty);
        set => SetValue(ActionButtonContentProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="ActionButtonStyle" /> property.
    /// </summary>
    public static readonly StyledProperty<ButtonStyle> ActionButtonStyleProperty =
        AvaloniaProperty.Register<Toast, ButtonStyle>(nameof(ActionButtonStyle));

    /// <summary>
    ///     Gets or sets the visual style of the action button.
    /// </summary>
    public ButtonStyle ActionButtonStyle
    {
        get => GetValue(ActionButtonStyleProperty);
        set => SetValue(ActionButtonStyleProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="CanDismissByClicking" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanDismissByClickingProperty =
        AvaloniaProperty.Register<Toast, bool>(nameof(CanDismissByClicking));

    /// <summary>
    ///     Gets or sets a value indicating whether clicking anywhere on the toast card
    ///     raises the <see cref="CloseRequested" /> event.
    /// </summary>
    public bool CanDismissByClicking
    {
        get => GetValue(CanDismissByClickingProperty);
        set => SetValue(CanDismissByClickingProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="CanDismiss" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanDismissProperty =
        AvaloniaProperty.Register<Toast, bool>(nameof(CanDismiss), true);

    /// <summary>
    ///     Gets or sets a value indicating whether the close button is visible.
    /// </summary>
    public bool CanDismiss
    {
        get => GetValue(CanDismissProperty);
        set => SetValue(CanDismissProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Command"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<Toast, ICommand?>(nameof(Command));

    /// <summary>
    ///     Gets or sets the command to execute when the toast is closed
    ///     (via close button, card click with <see cref="CanDismissByClicking" />, or programmatic dismiss).
    ///     The command receives the associated <see cref="ToastResult" /> as its parameter.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="ActionCommand"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> ActionCommandProperty =
        AvaloniaProperty.Register<Toast, ICommand?>(nameof(ActionCommand));

    /// <summary>
    ///     Gets or sets the command to execute when the action button is clicked.
    /// </summary>
    public ICommand? ActionCommand
    {
        get => GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="ActionCommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ActionCommandParameterProperty =
        AvaloniaProperty.Register<Toast, object?>(nameof(ActionCommandParameter));

    /// <summary>
    ///     Gets or sets the parameter to pass to the <see cref="ActionCommand"/> when it is executed.
    /// </summary>
    public object? ActionCommandParameter
    {
        get => GetValue(ActionCommandParameterProperty);
        set => SetValue(ActionCommandParameterProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="DismissCommand"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> DismissCommandProperty =
        AvaloniaProperty.Register<Toast, ICommand?>(nameof(DismissCommand));

    /// <summary>
    ///     Gets or sets the command to execute when the toast is dismissed (via close button, card click with <see cref="CanDismissByClicking" />, or programmatic dismiss).
    /// </summary>
    public ICommand? DismissCommand
    {
        get => GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="DismissCommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> DismissCommandParameterProperty =
        AvaloniaProperty.Register<Toast, object?>(nameof(DismissCommandParameter));

    /// <summary>
    ///     Gets or sets the parameter to pass to the <see cref="DismissCommand"/> when it is executed.
    /// </summary>
    public object? DismissCommandParameter
    {
        get => GetValue(DismissCommandParameterProperty);
        set => SetValue(DismissCommandParameterProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    ///     Raised when the user clicks the close button, or clicks the card when
    ///     <see cref="CanDismissByClicking" /> is <c>true</c>.
    /// </summary>
    public event EventHandler<ToastResult>? CloseRequested;

    /// <summary>
    ///     Raised when the user clicks the action button.
    /// </summary>
    public event EventHandler? ActionButtonClicked;

    #endregion

    #region Internal Fields (used by ToastHost)

    /// <summary>
    ///     Auto-dismiss duration. Set by <see cref="ToastHost" /> or
    ///     <see cref="ToastBuilder" />. When <see cref="TimeSpan.Zero" />,
    ///     the toast will not auto-dismiss.
    /// </summary>
    internal TimeSpan Duration { get; set; }

    /// <summary>
    ///     Per-toast screen position override. When <c>null</c>,
    ///     <see cref="ToastHost" /> uses its own <see cref="Position" />.
    /// </summary>
    internal ToastPosition? Position { get; set; }

    /// <summary>
    ///     Cancellation token source provided by callers.
    ///     Canceled when the toast is dismissed.
    /// </summary>
    internal CancellationTokenSource? DismissCts { get; set; }

    /// <summary>
    ///     Cancellation token source for the auto-dismiss timer.
    ///     Managed exclusively by <see cref="ToastHost" />.
    /// </summary>
    internal CancellationTokenSource? AutoDismissCts { get; set; }

    #endregion

    #region Template Event Handlers (IDisposable)

    private IDisposable? _cardPressDisposable;
    private IDisposable? _actionButtonClickDisposable;
    private IDisposable? _closeButtonClickDisposable;

    #endregion

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _cardPressDisposable?.Dispose();
        _actionButtonClickDisposable?.Dispose();
        _closeButtonClickDisposable?.Dispose();

        _cardPressDisposable = e.NameScope.Get<Border>("PART_ToastCard")
            .AddDisposableHandler(PointerPressedEvent, ToastCardClickedHandler, RoutingStrategies.Tunnel);

        _actionButtonClickDisposable = e.NameScope.Get<Button>("PART_ActionButton")
            .AddDisposableHandler(
                Button.ClickEvent,
                (_, _) =>
                {
                    if (Command is { } command && command.CanExecute(ToastResult.ActionButtonClicked))
                        command.Execute(ToastResult.ActionButtonClicked);

                    if (ActionCommand is { } actionCommand && actionCommand.CanExecute(ActionCommandParameter))
                        actionCommand.Execute(ActionCommandParameter);

                    ActionButtonClicked?.Invoke(this, EventArgs.Empty);
                });

        _closeButtonClickDisposable = e.NameScope.Get<Button>("PART_CloseButton")
            .AddDisposableHandler(Button.ClickEvent, (_, _) => RequestClose(ToastResult.Dismissed));
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _cardPressDisposable?.Dispose();
        _actionButtonClickDisposable?.Dispose();
        _closeButtonClickDisposable?.Dispose();

        _cardPressDisposable = null;
        _actionButtonClickDisposable = null;
        _closeButtonClickDisposable = null;
    }

    #region Internal Helpers

    /// <summary>
    ///     Raises <see cref="CloseRequested" /> and executes <see cref="Command" />.
    ///     Called by <see cref="ToastHost" /> or by internal button handlers.
    /// </summary>
    /// <param name="result">The reason for the close request.</param>
    internal void RequestClose(ToastResult result)
    {
        if (Command is { } command && command.CanExecute(result)) command.Execute(result);

        switch (result)
        {
            case ToastResult.ActionButtonClicked when ActionCommand is { } actionCommand && actionCommand.CanExecute(ActionCommandParameter):
                actionCommand.Execute(ActionCommandParameter);
                break;
            case ToastResult.Dismissed when DismissCommand is { } dismissCommand && dismissCommand.CanExecute(DismissCommandParameter):
                dismissCommand.Execute(DismissCommandParameter);
                break;
        }

        CloseRequested?.Invoke(this, result);
    }

    #endregion

    #region Private Handlers

    private DateTimeOffset _lastProgressUpdate = DateTimeOffset.MinValue;

    private void ProgressChangedHandler(object? sender, double e)
    {
        if (DateTimeOffset.Now - _lastProgressUpdate < TimeSpan.FromMilliseconds(100)) return;

        Dispatcher.UIThread.PostOnDemand(
            () =>
            {
                ProgressValue = e;
                _lastProgressUpdate = DateTimeOffset.Now;
            },
            DispatcherPriority.Normal);
    }

    private void ToastCardClickedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (CanDismissByClicking) RequestClose(ToastResult.Dismissed);
    }

    #endregion

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty) IsEmptyContent = Content == null;
    }
}