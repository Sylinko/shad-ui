using Avalonia.Controls;
using Avalonia.Threading;
using ShadUI.Extensions;

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

    public Task<DialogResult> ShowAsync(CancellationToken cancellationToken = default)
    {
        if (_control == null) throw new InvalidOperationException("Dialog control is not set.");

        var tcs = new TaskCompletionSource<DialogResult>();
        var callback = Callback;
        callback += result => tcs.TrySetResult(result);

        _manager._callbacks.TryAdd(_control, callback);
        _manager.Show(_control, Options);

        if (cancellationToken.CanBeCanceled) cancellationToken.Register(() => Dispatcher.UIThread.InvokeOnDemand(() => _manager.Close(_control)));

        return tcs.Task;
    }
}