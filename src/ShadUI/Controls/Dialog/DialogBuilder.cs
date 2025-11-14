using ShadUI.Extensions;
using Avalonia.Threading;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Builds a simple dialog.
/// </summary>
public sealed class DialogBuilder
{
    private readonly DialogManager _manager;
    private readonly SimpleDialog _dialog;

    public DialogOptions Options { get; } = new();

    internal DialogBuilder(DialogManager manager, object content, object? title = null)
    {
        _manager = manager;
        _dialog = new SimpleDialog(manager)
        {
            Title = title,
            Content = content
        };
    }

    /// <summary>
    ///     Sets the title of the dialog.
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public DialogBuilder WithTitle(string title)
    {
        _dialog.Title = title;
        return this;
    }

    /// <summary>
    ///     Sets the primary button of the dialog.
    /// </summary>
    /// <param name="content">The button content</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Primary" /></param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithPrimaryButton(
        object content,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Primary)
    {
        _dialog.PrimaryButtonContent = content;
        _dialog.PrimaryButtonStyle = buttonStyle;
        return this;
    }

    /// <summary>
    ///     Sets the secondary button of the dialog.
    /// </summary>
    /// <param name="content">The button content</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Secondary" /></param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithSecondaryButton(
        object content,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Secondary)
    {
        _dialog.SecondaryButtonContent = content;
        _dialog.SecondaryButtonStyle = buttonStyle;
        return this;
    }

    /// <summary>
    ///     Sets the tertiary button of the dialog.
    /// </summary>
    /// <param name="content">The button content</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Outline" /></param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithTertiaryButton(
        object content,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Outline)
    {
        _dialog.TertiaryButtonContent = content;
        _dialog.TertiaryButtonStyle = buttonStyle;
        return this;
    }

    /// <summary>
    ///     Sets the cancel button of the dialog.
    /// </summary>
    /// <param name="content">The button content</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Outline" /></param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithCancelButton(
        object content,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Outline)
    {
        _dialog.CancelButtonContent = content;
        _dialog.CancelButtonStyle = buttonStyle;
        return this;
    }

    /// <summary>
    ///     Makes the dialog dismissible by clicking outside or pressing escape.
    /// </summary>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder Dismissible()
    {
        Options.Dismissible = true;
        return this;
    }

    /// <summary>
    ///     Sets the maximum width of the dialog.
    /// </summary>
    /// <param name="maxWidth">The maximum width in pixels</param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithMaxWidth(double maxWidth)
    {
        Options.MaxWidth = maxWidth;
        return this;
    }

    /// <summary>
    ///     Sets the minimum width of the dialog.
    /// </summary>
    /// <param name="minWidth">The minimum width in pixels</param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithMinWidth(double minWidth)
    {
        Options.MinWidth = minWidth;
        return this;
    }

    /// <summary>
    ///     Shows the dialog and returns the result.
    /// </summary>
    public Task<DialogResult> ShowAsync(CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<DialogResult>();
        _manager.Show(_dialog, r => tcs.TrySetResult(r), Options);

        if (cancellationToken.CanBeCanceled) cancellationToken.Register(() => Dispatcher.UIThread.InvokeOnDemand(() => _manager.Close(_dialog)));

        return tcs.Task;
    }
}