using Avalonia.Controls;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Dialog service for showing dialogs.
/// </summary>
public sealed class DialogManager
{
    internal event EventHandler<DialogShownEventArgs>? OnDialogShown;
    internal event EventHandler<DialogClosedEventArgs>? OnDialogClosed;
    internal IReadOnlyDictionary<Control, DialogOptions> Dialogs => _dialogs;

    private readonly Dictionary<Control, DialogOptions> _dialogs = [];
    private readonly Dictionary<Control, Action<DialogResult>> _callbacks = [];

    /// <summary>
    ///     Creates a simple dialog.
    /// </summary>
    /// <param name="content">The dialog content</param>
    /// <param name="title">The dialog title</param>
    /// <returns>A new instance of <see cref="DialogBuilder" /></returns>
    public DialogBuilder CreateDialog(object content, object? title = null) => new(this, content, title);

    /// <summary>
    ///     Creates a dialog with a custom context.
    /// </summary>
    /// <param name="control">The dialog content</param>
    public CustomDialogBuilder CreateCustomDialog(Control control) => new(this, control);

    /// <summary>
    ///     Shows a dialog with the provided options.
    /// </summary>
    /// <param name="control">Control to be shown as dialog</param>
    /// <param name="callback">Callback when dialog is closed</param>
    /// <param name="options">Dialog options</param>
    internal void Show(Control control, Action<DialogResult> callback, DialogOptions options)
    {
        if (_dialogs.Count > 0)
        {
            if (_dialogs.Any(x => x.Key == control)) return;

            var last = _dialogs.Last();
            if (last.Key != control)
            {
                OnDialogClosed?.Invoke(this, new DialogClosedEventArgs(last.Key, true));
            }
        }

        _dialogs.TryAdd(control, options);
        _callbacks.TryAdd(control, callback);
        OnDialogShown?.Invoke(this, new DialogShownEventArgs(control, options));
    }

    internal void CloseDialog(Control control, DialogResult result)
    {
        _dialogs.Remove(control);
        OnDialogClosed?.Invoke(this, new DialogClosedEventArgs(control, _dialogs.Count > 0));
        InvokeCallBacks(control, result);
    }

    internal void OpenLast()
    {
        if (_dialogs.Count == 0) return;

        var lastDialog = _dialogs.Last();
        OnDialogShown?.Invoke(this, new DialogShownEventArgs(lastDialog.Key, lastDialog.Value));
    }

    internal void RemoveLast(DialogResult result)
    {
        if (_dialogs.Count == 0) return;

        var lastDialog = _dialogs.Last();
        CloseDialog(lastDialog.Key, result);
        InvokeCallBacks(lastDialog.Key, result);
    }

    private void InvokeCallBacks(Control control, DialogResult result)
    {
        if (_callbacks.Remove(control, out var callback))
        {
            callback.Invoke(result);
        }
    }

    /// <summary>
    ///     Closes the dialog associated with the specified context and invokes the appropriate callbacks.
    /// </summary>
    /// <param name="control">The control of the dialog to close.</param>
    /// <param name="result"></param>
    public void Close(Control control, DialogResult result = DialogResult.Cancel)
    {
        var dialogs = _dialogs.Where(x => Equals(x.Key, control)).ToList();

        foreach (var dialog in dialogs) CloseDialog(dialog.Key, result);

        InvokeCallBacks(control, result);
        OpenLast();
    }

    /// <summary>
    ///     Closes all open dialogs and invokes their callbacks.
    /// </summary>
    /// <param name="result"></param>
    public void CloseAll(DialogResult result = DialogResult.Cancel)
    {
        var dialogs = _dialogs.Keys.ToList();
        foreach (var dialog in dialogs)
        {
            CloseDialog(dialog, result);
            InvokeCallBacks(dialog, result);
        }
    }

    internal event EventHandler<bool>? AllowDismissChanged;

    /// <summary>
    ///     Disables the ability to dismiss dialogs. This overrides the <see cref="DialogHost.Dismissible" /> property of the
    ///     <see cref="DialogHost" />.
    /// </summary>
    public void PreventDismissal()
    {
        AllowDismissChanged?.Invoke(this, false);
    }

    /// <summary>
    ///     Enables the ability to dismiss dialogs. This overrides the <see cref="DialogHost.Dismissible" /> property of the
    ///     <see cref="DialogHost" />.
    /// </summary>
    public void AllowDismissal()
    {
        AllowDismissChanged?.Invoke(this, true);
    }
}

internal sealed class DialogShownEventArgs(Control control, DialogOptions options) : EventArgs
{
    public Control Control { get; } = control;
    public DialogOptions Options { get; } = options;
}

internal sealed class DialogClosedEventArgs(Control control, bool replaceExisting = false) : EventArgs
{
    public Control Control { get; } = control;
    public bool ReplaceExisting { get; } = replaceExisting;
}