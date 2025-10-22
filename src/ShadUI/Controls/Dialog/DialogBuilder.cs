using Avalonia.Controls;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Builds a dialog.
/// </summary>
public sealed class DialogBuilder
{
    private readonly DialogManager _manager;

    internal DialogBuilder(DialogManager manager)
    {
        _manager = manager;
    }

    internal Action<DialogResult>? Callback { get; set; }
    internal DialogOptions Options { get; } = new();

    private Control? _control;

    internal DialogBuilder CreateDialog(Control control)
    {
        _control = control;
        return this;
    }

    public Task<DialogResult> ShowAsync()
    {
        if (_control == null) throw new InvalidOperationException("Dialog control is not set.");

        var tcs = new TaskCompletionSource<DialogResult>();
        var callback = Callback;
        callback += result => tcs.SetResult(result);

        _manager.Callbacks.TryAdd(_control, callback);
        _manager.Show(_control, Options);

        return tcs.Task;
    }
}