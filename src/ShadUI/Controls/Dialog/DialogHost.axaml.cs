using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Threading;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Hosts the dialog stack belonging to one TopLevel and participates in global routing through
///     <see cref="DialogManager"/> while attached to the visual tree.
/// </summary>
[TemplatePart("PART_Layers", typeof(Panel))]
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

    /// <summary>
    /// The page behind the host, never a visual ancestor of it. ShadWindow supplies
    /// its page root; other layouts can bind their own sibling explicitly.
    /// </summary>
    public static readonly StyledProperty<Control?> BackgroundRootProperty =
        AvaloniaProperty.Register<DialogHost, Control?>(nameof(BackgroundRoot));

    /// <inheritdoc cref="BackgroundRootProperty"/>
    public Control? BackgroundRoot
    {
        get => GetValue(BackgroundRootProperty);
        set => SetValue(BackgroundRootProperty, value);
    }

    /// <summary>
    /// Defines the height of the window title bar covered by this host.
    /// </summary>
    public static readonly StyledProperty<double> TitleBarHeightProperty =
        AvaloniaProperty.Register<DialogHost, double>(nameof(TitleBarHeight));

    /// <summary>
    /// The height of the draggable strip at the top of the backdrop. Zero for hosts outside the title bar.
    /// </summary>
    public double TitleBarHeight
    {
        get => GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    private readonly List<DialogLayer> _dialogs = [];
    private Panel? _layers;

    /// <summary>Creates a simple dialog pinned to this host.</summary>
    public SimpleDialogBuilder CreateDialog(object content, object? title = null) => new(this, content, title);

    /// <summary>Creates a custom dialog pinned to this host.</summary>
    public CustomDialogBuilder CreateCustomDialog(Control control) => new(this, control);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_layers is not null)
        {
            foreach (var layer in _layers.Children.OfType<DialogLayer>().ToArray())
            {
                if (!_dialogs.Contains(layer)) ReleaseLayer(layer);
            }

            _layers.Children.Clear();
        }

        base.OnApplyTemplate(e);
        _layers = e.NameScope.Get<Panel>("PART_Layers");
        foreach (var layer in _dialogs) _layers.Children.Add(layer);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BackgroundRootProperty)
        {
            if (change.OldValue is Control oldRoot)
            {
                oldRoot.Classes.Remove("DialogBackgroundCovered");
                oldRoot.Classes.Remove("DialogBackgroundRoot");
            }
            if (BackgroundRoot is { } root)
            {
                root.Classes.Add("DialogBackgroundRoot");
                root.Classes.Set("DialogBackgroundCovered", HasOpenedDialog);
            }
        }
        else if (change.Property == HasOpenedDialogProperty)
        {
            BackgroundRoot?.Classes.Set("DialogBackgroundCovered", HasOpenedDialog);
        }
        else if (_dialogs is { Count: > 0 } &&
                 (change.Property == DismissibleProperty ||
                     change.Property == DialogMinWidthProperty ||
                     change.Property == DialogMaxWidthProperty))
        {
            var layer = _dialogs[^1];
            if (change.Property == DismissibleProperty) layer.Options.Dismissible = Dismissible;
            if (change.Property == DialogMinWidthProperty) layer.Options.MinWidth = DialogMinWidth;
            if (change.Property == DialogMaxWidthProperty) layer.Options.MaxWidth = DialogMaxWidth;
            layer.ApplyOptions();
        }
        else if (change.Property == DialogProperty && change.NewValue is null && change.OldValue is Control previous)
        {
            // Clearing the presented content explicitly also releases its exiting visual.
            ReleaseClosingLayer(previous);
        }
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
        // No animation can finish on a detached host. Release controls for reuse by another window.
        CloseAll(DialogResult.Cancel, false);

        if (_layers is not null)
        {
            foreach (var layer in _layers.Children.OfType<DialogLayer>().ToArray()) ReleaseLayer(layer);
        }

        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e is { Handled: false, Key: Key.Escape } && HasOpenedDialog && Dismissible)
        {
            Close(_dialogs[^1].View);
            e.Handled = true;
        }
    }

    internal void RequestDismiss(DialogLayer layer)
    {
        if (_dialogs.Count > 0 && ReferenceEquals(_dialogs[^1], layer) && Dismissible) Close(layer.View);
    }

    /// <summary>Adds a dialog above the current entry without detaching its parent.</summary>
    internal bool Show(Control control, Action<DialogResult>? callback, DialogOptions options)
    {
        Dispatcher.UIThread.VerifyAccess();
        if (_dialogs.Any(layer => ReferenceEquals(layer.View, control))) return false;

        if (_layers is not null)
        {
            foreach (var closing in _layers.Children.OfType<DialogLayer>().Where(layer => layer.IsClosing).ToArray())
            {
                ReleaseLayer(closing);
            }
        }

        var opener = TopLevel.GetTopLevel(this)?.FocusManager.GetFocusedElement();
        if (_dialogs.Count > 0) _dialogs[^1].SetCovered(true);

        var layer = new DialogLayer(this, control, options, callback, opener);
        layer.Classes.Set("first", _dialogs.Count == 0);
        _dialogs.Add(layer);

        if (control is SimpleDialog dialog) dialog.CloseRequested = result => Close(control, result);
        Present(layer);
        _layers?.Children.Add(layer);
        return true;
    }

    private void Present(DialogLayer layer)
    {
        Dialog = layer.View;
        DialogMaxWidth = layer.Options.MaxWidth;
        DialogMinWidth = layer.Options.MinWidth;
        Dismissible = layer.Options.Dismissible;
        HasOpenedDialog = true;
        IsDialogOpen = true;
    }

    /// <summary>Closes a specific entry, keeping the visible card for its exit animation.</summary>
    public void Close(Control control, DialogResult result = DialogResult.Cancel) => Close(control, result, true);

    /// <summary>Skip the exit when the caller needs to move the same control to another host immediately.</summary>
    public void Close(Control control, DialogResult result, bool shouldAnimate)
    {
        Dispatcher.UIThread.VerifyAccess();
        var index = _dialogs.FindIndex(layer => ReferenceEquals(layer.View, control));
        if (index < 0)
        {
            if (!shouldAnimate) ReleaseClosingLayer(control);
            return;
        }

        var layer = _dialogs[index];
        var wasCurrent = index == _dialogs.Count - 1;
        _dialogs.RemoveAt(index);
        if (control is SimpleDialog dialog) dialog.CloseRequested = null;
        if (!wasCurrent)
        {
            // Preserve the way back if a cancelled parent disappears under an open child.
            _dialogs[index].Opener = layer.Opener;
        }
        else if (_dialogs.Count > 0)
        {
            var parent = _dialogs[^1];
            parent.SetCovered(false);
            Present(parent);
        }
        else
        {
            IsDialogOpen = false;
            HasOpenedDialog = false;
        }

        Retire(layer, wasCurrent && shouldAnimate);
        if (wasCurrent) RestoreFocus(layer.Opener);
        // Complete only after state is coherent: callbacks can open another dialog immediately.
        layer.Complete(result);
    }

    /// <summary>Closes every entry owned by this host and completes each callback once.</summary>
    public void CloseAll(DialogResult result = DialogResult.Cancel) => CloseAll(result, true);

    private void CloseAll(DialogResult result, bool shouldAnimate)
    {
        Dispatcher.UIThread.VerifyAccess();
        if (_dialogs.Count == 0) return;

        var entries = _dialogs.ToArray();
        _dialogs.Clear();
        IsDialogOpen = false;
        HasOpenedDialog = false;
        foreach (var layer in entries)
        {
            if (layer.View is SimpleDialog dialog) dialog.CloseRequested = null;
            Retire(layer, shouldAnimate && ReferenceEquals(layer, entries[^1]));
        }

        if (shouldAnimate) RestoreFocus(entries[0].Opener);
        foreach (var layer in entries) layer.Complete(result);
    }

    private void RestoreFocus(IInputElement? opener)
    {
        // The command that opened a child can still be disabled until ShowAsync completes.
        // Let its continuation run first, without stealing focus from a replacement dialog.
        var target = _dialogs.LastOrDefault();
        Dispatcher.UIThread.Post(
            () =>
            {
                if (!ReferenceEquals(target, _dialogs.LastOrDefault()) || !this.IsAttachedToVisualTree()) return;
                if (opener is Control control && TopLevel.GetTopLevel(control) == TopLevel.GetTopLevel(this) && control.Focus()) return;
                target?.Focus();
            },
            DispatcherPriority.Background);
    }

    private void Retire(DialogLayer layer, bool shouldAnimate)
    {
        if (!shouldAnimate)
        {
            ReleaseLayer(layer);
            return;
        }

        // Keep Dialog readable until the final fade completes, including for unthemed hosts.
        layer.StartExit(!HasOpenedDialog, () => ReleaseLayer(layer));
    }

    private void ReleaseClosingLayer(Control view)
    {
        if (_layers?.Children.OfType<DialogLayer>().FirstOrDefault(l => l.IsClosing && ReferenceEquals(l.View, view)) is { } layer)
        {
            ReleaseLayer(layer);
        }
    }

    private void ReleaseLayer(DialogLayer layer)
    {
        layer.Release();
        _layers?.Children.Remove(layer);
        if (!HasOpenedDialog && ReferenceEquals(Dialog, layer.View)) Dialog = null;
    }

    /// <summary>Prevents dismissal of the current entry by the host chrome.</summary>
    public void PreventDismissal()
    {
        if (_dialogs.Count > 0) Dismissible = false;
    }

    /// <summary>Allows dismissal of the current entry by the host chrome.</summary>
    public void AllowDismissal()
    {
        if (_dialogs.Count > 0) Dismissible = true;
    }
}
