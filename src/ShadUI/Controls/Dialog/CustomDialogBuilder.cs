using Avalonia.Controls;
using Avalonia.Threading;
using ShadUI.Extensions;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Builds a dialog with a custom control.
/// </summary>
public sealed class CustomDialogBuilder
{
    private readonly DialogManager _manager;
    private readonly DialogOptions _options = new();
    private readonly Control _control;

    private Action<DialogResult>? _callback;

    internal CustomDialogBuilder(DialogManager manager, Control control)
    {
        _manager = manager;
        _control = control;
    }

    /// <summary>
    ///     Sets the callback for the dialog.
    /// </summary>
    /// <param name="callback">The method that is called when the dialog is closed</param>
    /// <returns>The modified <see cref="CustomDialogBuilder" /> instance</returns>
    public CustomDialogBuilder WithCallback(Action<DialogResult> callback)
    {
        _callback = callback;
        return this;
    }

    /// <summary>
    ///     Makes the dialog dismissible by clicking outside or pressing escape. If set to true, this will take precedence over
    ///     toggling <see cref="DialogManager.PreventDismissal()" />
    /// </summary>
    /// <returns>The modified <see cref="CustomDialogBuilder" /> instance</returns>
    public CustomDialogBuilder Dismissible()
    {
        _options.Dismissible = true;
        return this;
    }

    /// <summary>
    ///     Sets the maximum width of the dialog.
    /// </summary>
    /// <param name="maxWidth">The maximum width in pixels</param>
    /// <returns>The modified <see cref="CustomDialogBuilder" /> instance</returns>
    public CustomDialogBuilder WithMaxWidth(double maxWidth)
    {
        _options.MaxWidth = maxWidth;
        return this;
    }

    /// <summary>
    ///     Sets the minimum width of the dialog.
    /// </summary>
    /// <param name="minWidth">The minimum width in pixels</param>
    /// <returns>The modified <see cref="CustomDialogBuilder" /> instance</returns>
    public CustomDialogBuilder WithMinWidth(double minWidth)
    {
        _options.MinWidth = minWidth;
        return this;
    }

    public Task<DialogResult> ShowAsync(CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<DialogResult>();
        var callback = _callback;
        callback += result => tcs.TrySetResult(result);

        _manager.Show(_control, callback, _options);

        if (cancellationToken.CanBeCanceled) cancellationToken.Register(() => Dispatcher.UIThread.Invoke(() => _manager.Close(_control)));

        return tcs.Task;
    }

    /// <summary>
    ///     Shows the dialog with the provided options.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Show(CancellationToken cancellationToken = default)
    {
        _manager.Show(_control, _callback, _options);

        if (cancellationToken.CanBeCanceled) cancellationToken.Register(() => Dispatcher.UIThread.Invoke(() => _manager.Close(_control)));
    }

    /// <summary>
    ///     Shows the dialog with the provided options and returns a <see cref="DialogResult" /> when the dialog is closed.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public DialogResult ShowDialog(CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<DialogResult>();
        var callback = _callback;
        callback += result => tcs.TrySetResult(result);

        _manager.Show(_control, callback, _options);

        if (cancellationToken.CanBeCanceled) cancellationToken.Register(() => Dispatcher.UIThread.Invoke(() => _manager.Close(_control)));

        return tcs.Task.WaitOnDispatcherFrame();
    }
}