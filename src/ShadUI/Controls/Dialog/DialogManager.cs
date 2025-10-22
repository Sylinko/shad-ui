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

    internal readonly Dictionary<Control, DialogOptions> Dialogs = [];

    /// <summary>
    ///     Shows a dialog with the provided options.
    /// </summary>
    /// <param name="control">Control to be shown as dialog</param>
    /// <param name="options">Dialog options</param>
    internal void Show(Control control, DialogOptions options)
    {
        if (Dialogs.Count > 0)
        {
            if (control is SimpleDialog simple)
            {
                var existingSimpleDialog = Dialogs.FirstOrDefault(x => x.Key is SimpleDialog d && d.Id == simple.Id)
                    .Key;

                if (existingSimpleDialog is not null) return;
            }

            var existingCustomDialog =
                Dialogs.FirstOrDefault(x =>
                    x.Key.DataContext?.GetType() == control.DataContext?.GetType()).Key;
            if (existingCustomDialog is not null) return;

            var last = Dialogs.Last();
            if (last.Key != control)
            {
                OnDialogClosed?.Invoke(this, new DialogClosedEventArgs { ReplaceExisting = true, Control = last.Key });
            }
        }

        Dialogs.TryAdd(control, options);
        OnDialogShown?.Invoke(this, new DialogShownEventArgs { Control = control, Options = options });
    }

    internal void CloseDialog(Control control)
    {
        Dialogs.Remove(control);

        OnDialogClosed?.Invoke(this, new DialogClosedEventArgs
        {
            ReplaceExisting = Dialogs.Count > 0,
            Control = control
        });
    }

    internal void OpenLast()
    {
        if (Dialogs.Count == 0) return;

        var lastDialog = Dialogs.Last();
        OnDialogShown?.Invoke(this, new DialogShownEventArgs { Control = lastDialog.Key, Options = lastDialog.Value });
    }

    internal void RemoveLast()
    {
        if (Dialogs.Count == 0) return;

        var lastDialog = Dialogs.Last();
        CloseDialog(lastDialog.Key);

        InvokeCallBacks(lastDialog.Key, DialogResult.Cancel);
    }

    internal readonly Dictionary<Control, Action<DialogResult>> Callbacks = [];

    private void InvokeCallBacks(Control control, DialogResult result)
    {
        if (Callbacks.Remove(control, out var callback))
        {
            callback.Invoke(result);
        }
    }

    /// <summary>
    ///     Closes the dialog associated with the specified context and invokes the appropriate callbacks.
    /// </summary>
    /// <param name="control">The control of the dialog to close.</param>
    /// <param name="result"></param>
    /// <param name="closeAll"></param>
    public void Close(Control control, DialogResult result = DialogResult.Cancel, bool closeAll = false)
    {
        var dialogs = Dialogs.Where(x => Equals(x.Key, control)).ToList();

        if (closeAll) RemoveAll();

        foreach (var dialog in dialogs) CloseDialog(dialog.Key);

        InvokeCallBacks(control, result);

        if (!closeAll) OpenLast();
    }

    /// <summary>
    ///     Closes all open dialogs and invokes their callbacks.
    /// </summary>
    /// <param name="result"></param>
    public void CloseAll(DialogResult result = DialogResult.Cancel)
    {
        var dialogs = Dialogs.Keys.ToList();
        foreach (var dialog in dialogs)
        {
            CloseDialog(dialog);
            InvokeCallBacks(dialog, result);
        }
    }

    private void RemoveAll()
    {
        Dialogs.Clear();
        Callbacks.Clear();
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

internal sealed class DialogShownEventArgs : EventArgs
{
    public Control Control { get; set; } = null!;
    public DialogOptions Options { get; set; } = null!;
}

internal sealed class DialogClosedEventArgs : EventArgs
{
    public bool ReplaceExisting { get; set; }
    public Control Control { get; set; } = null!;
}