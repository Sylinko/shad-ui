using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Hosts the dialog stack belonging to one TopLevel and participates in global routing through
///     <see cref="DialogManager"/> while attached to the visual tree.
/// </summary>
[TemplatePart("PART_DialogBackground", typeof(Border))]
[TemplatePart("PART_CloseButton", typeof(Button))]
public class DialogHost : TemplatedControl
{
    /// <summary>
    /// Defines the currently presented dialog content.
    /// </summary>
    public static readonly StyledProperty<Control?> DialogProperty =
        AvaloniaProperty.Register<DialogHost, Control?>(nameof(Dialog));

    /// <summary>
    /// Gets or sets the currently presented dialog content.
    /// </summary>
    public Control? Dialog
    {
        get => GetValue(DialogProperty);
        set => SetValue(DialogProperty, value);
    }

    /// <summary>
    /// Defines whether the current dialog is in its open visual state.
    /// </summary>
    public static readonly StyledProperty<bool> IsDialogOpenProperty =
        AvaloniaProperty.Register<DialogHost, bool>(nameof(IsDialogOpen));

    /// <summary>
    /// Gets or sets whether the current dialog is in its open visual state.
    /// </summary>
    public bool IsDialogOpen
    {
        get => GetValue(IsDialogOpenProperty);
        set => SetValue(IsDialogOpenProperty, value);
    }

    /// <summary>
    /// Defines the maximum width of the current dialog.
    /// </summary>
    public static readonly StyledProperty<double> DialogMaxWidthProperty =
        AvaloniaProperty.Register<DialogHost, double>(nameof(DialogMaxWidth), double.MaxValue);

    /// <summary>
    /// Gets or sets the maximum width of the current dialog.
    /// </summary>
    public double DialogMaxWidth
    {
        get => GetValue(DialogMaxWidthProperty);
        set => SetValue(DialogMaxWidthProperty, value);
    }

    /// <summary>
    /// Defines the minimum width of the current dialog.
    /// </summary>
    public static readonly StyledProperty<double> DialogMinWidthProperty =
        AvaloniaProperty.Register<DialogHost, double>(nameof(DialogMinWidth), double.MinValue);

    /// <summary>
    /// Gets or sets the minimum width of the current dialog.
    /// </summary>
    public double DialogMinWidth
    {
        get => GetValue(DialogMinWidthProperty);
        set => SetValue(DialogMinWidthProperty, value);
    }

    /// <summary>
    /// Defines whether the current dialog can be dismissed by the host chrome.
    /// </summary>
    public static readonly StyledProperty<bool> DismissibleProperty =
        AvaloniaProperty.Register<DialogHost, bool>(nameof(Dismissible), true);

    /// <summary>
    /// Gets or sets whether the current dialog can be dismissed by the host chrome.
    /// </summary>
    public bool Dismissible
    {
        get => GetValue(DismissibleProperty);
        set => SetValue(DismissibleProperty, value);
    }

    /// <summary>
    /// Defines whether this host retains any dialog entries.
    /// </summary>
    public static readonly DirectProperty<DialogHost, bool> HasOpenedDialogProperty =
        AvaloniaProperty.RegisterDirect<DialogHost, bool>(nameof(HasOpenedDialog), o => o.HasOpenedDialog);

    /// <summary>
    /// Gets or sets whether this host retains any dialog entries.
    /// </summary>
    public bool HasOpenedDialog
    {
        get;
        private set => SetAndRaise(HasOpenedDialogProperty, ref field, value);
    }

    /// <summary>
    /// Defines whether clicking the overlay may request dismissal.
    /// </summary>
    public static readonly StyledProperty<bool> CanDismissWithBackgroundClickProperty =
        AvaloniaProperty.Register<DialogHost, bool>(nameof(CanDismissWithBackgroundClick), true);

    /// <summary>
    /// Gets or sets whether clicking the overlay may request dismissal.
    /// </summary>
    public bool CanDismissWithBackgroundClick
    {
        get => GetValue(CanDismissWithBackgroundClickProperty);
        set => SetValue(CanDismissWithBackgroundClickProperty, value);
    }

    private readonly List<DialogEntry> _dialogs = [];
    private Border? _dialogBackground;
    private Button? _closeButton;

    /// <summary>Creates a simple dialog pinned to this host.</summary>
    public SimpleDialogBuilder CreateDialog(object content, object? title = null) => new(this, content, title);

    /// <summary>Creates a custom dialog pinned to this host.</summary>
    public CustomDialogBuilder CreateCustomDialog(Control control) => new(this, control);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_dialogBackground is not null) _dialogBackground.PointerPressed -= HandleBackgroundPointerPressed;
        if (_closeButton is not null) _closeButton.Click -= HandleCloseButtonClick;

        base.OnApplyTemplate(e);

        _dialogBackground = e.NameScope.Find<Border>("PART_DialogBackground");
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        if (_dialogBackground is not null) _dialogBackground.PointerPressed += HandleBackgroundPointerPressed;
        if (_closeButton is not null) _closeButton.Click += HandleCloseButtonClick;
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        DialogManager.Register(this);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DialogManager.Unregister(this);

        // A detached host can no longer receive user input. Complete every pending builder now so
        // callers never retain a task and an arbitrary dialog control after its window closes.
        CloseAll();
        base.OnDetachedFromVisualTree(e);
    }

    private void HandleBackgroundPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (CanDismissWithBackgroundClick) CloseCurrent();
    }

    private void HandleCloseButtonClick(object? sender, RoutedEventArgs e) =>
        CloseCurrent();

    private void CloseCurrent()
    {
        if (!Dismissible || _dialogs.Count == 0) return;
        Close(_dialogs[^1].Control);
    }

    /// <summary>Adds a dialog to this host's stack and presents it as the current entry.</summary>
    internal bool Show(Control control, Action<DialogResult>? callback, DialogOptions options)
    {
        Dispatcher.UIThread.CheckAccess();

        if (_dialogs.Any(entry => ReferenceEquals(entry.Control, control))) return false;

        if (_dialogs.Count > 0) IsDialogOpen = false;

        var entry = new DialogEntry(control, options, callback);
        _dialogs.Add(entry);
        if (control is SimpleDialog simpleDialog)
            simpleDialog.CloseRequested = result => Close(simpleDialog, result);

        Present(entry);
        return true;
    }

    private void Present(DialogEntry entry)
    {
        Dialog = entry.Control;
        DialogMaxWidth = entry.Options.MaxWidth;
        DialogMinWidth = entry.Options.MinWidth;
        Dismissible = entry.Options.Dismissible;
        HasOpenedDialog = true;
        IsDialogOpen = true;
    }

    /// <summary>Closes a specific dialog owned by this host.</summary>
    public void Close(Control control, DialogResult result = DialogResult.Cancel)
    {
        Dispatcher.UIThread.CheckAccess();

        var index = _dialogs.FindIndex(entry => ReferenceEquals(entry.Control, control));
        if (index < 0) return;

        var entry = _dialogs[index];
        var wasCurrent = index == _dialogs.Count - 1;
        _dialogs.RemoveAt(index);
        if (control is SimpleDialog simpleDialog) simpleDialog.CloseRequested = null;

        if (wasCurrent)
        {
            IsDialogOpen = false;
            if (_dialogs.Count > 0)
            {
                Present(_dialogs[^1]);
            }
            else
            {
                HasOpenedDialog = false;
                ClearDialogAfterAnimation(control);
            }
        }

        // Restore the host to a coherent visual state before invoking application code. A callback
        // may immediately open another dialog, and that new entry must not be overwritten here.
        entry.Callback?.Invoke(result);
    }

    /// <summary>Closes every dialog owned by this host and completes each callback once.</summary>
    public void CloseAll(DialogResult result = DialogResult.Cancel)
    {
        Dispatcher.UIThread.CheckAccess();
        if (_dialogs.Count == 0) return;

        var closedDialog = Dialog;
        var entries = _dialogs.ToList();
        _dialogs.Clear();
        IsDialogOpen = false;
        HasOpenedDialog = false;

        foreach (var entry in entries)
        {
            if (entry.Control is SimpleDialog simpleDialog) simpleDialog.CloseRequested = null;
            entry.Callback?.Invoke(result);
        }

        if (closedDialog is not null) ClearDialogAfterAnimation(closedDialog);
    }

    /// <summary>
    ///     Restores the current dialog's configured dismissibility after a temporary override.
    /// </summary>
    public void PreventDismissal()
    {
        if (_dialogs.Count > 0) Dismissible = false;
    }

    /// <summary>Temporarily enables host-chrome dismissal for the current dialog.</summary>
    public void AllowDismissal()
    {
        if (_dialogs.Count > 0) Dismissible = true;
    }

    private async void ClearDialogAfterAnimation(object closedDialog)
    {
        try
        {
            await Task.Delay(200);
            if (_dialogs.Count == 0 && !IsDialogOpen && ReferenceEquals(Dialog, closedDialog))
                Dialog = null;
        }
        catch
        {
            // The delayed clear is purely visual cleanup and must not surface an async-void error.
        }
    }

    private readonly record struct DialogEntry(Control Control, DialogOptions Options, Action<DialogResult>? Callback);
}