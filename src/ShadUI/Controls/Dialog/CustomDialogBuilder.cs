using Avalonia.Controls;
using ShadUI.Extensions;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Builds a dialog with a custom control.
/// </summary>
public sealed class CustomDialogBuilder
{
    private readonly DialogHost? _host;
    private readonly DialogOptions _options = new();
    private readonly Control _control;

    internal CustomDialogBuilder(DialogHost? host, Control control)
    {
        _host = host;
        _control = control;
    }

    /// <summary>
    ///     Makes the dialog dismissible by clicking outside or pressing escape. If set to true, this will take precedence over
    ///     toggling <see cref="DialogHost.PreventDismissal" />
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
        return DialogManager.ShowAsync(_host, _control, _options, cancellationToken);
    }

    /// <summary>
    ///     Shows the dialog with the provided options.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Show(CancellationToken cancellationToken = default)
    {
        DialogManager.Show(_host, _control, null, _options, cancellationToken);
    }

    /// <summary>
    ///     Shows the dialog with the provided options and returns a <see cref="DialogResult" /> when the dialog is closed.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public DialogResult ShowDialog(CancellationToken cancellationToken = default)
    {
        return ShowAsync(cancellationToken).WaitOnDispatcherFrame();
    }
}