using System.ComponentModel;
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
    private readonly DialogOptions _options = new();

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
    /// <param name="callback"></param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="ButtonStyle.Primary" /></param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithPrimaryButton(
        object content,
        CancelEventHandler? callback = null,
        ButtonStyle buttonStyle = ButtonStyle.Primary)
    {
        _dialog.PrimaryButtonContent = content;
        _dialog.PrimaryCallback = callback;
        _dialog.PrimaryButtonStyle = buttonStyle;
        return this;
    }

    /// <summary>
    ///     Sets the secondary button of the dialog.
    /// </summary>
    /// <param name="content">The button content</param>
    /// <param name="callback"></param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="ButtonStyle.Secondary" /></param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithSecondaryButton(
        object content,
        CancelEventHandler? callback = null,
        ButtonStyle buttonStyle = ButtonStyle.Secondary)
    {
        _dialog.SecondaryButtonContent = content;
        _dialog.SecondaryCallback = callback;
        _dialog.SecondaryButtonStyle = buttonStyle;
        return this;
    }

    /// <summary>
    ///     Sets the tertiary button of the dialog.
    /// </summary>
    /// <param name="content">The button content</param>
    /// <param name="callback"></param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="ButtonStyle.Outline" /></param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithTertiaryButton(
        object content,
        CancelEventHandler? callback = null,
        ButtonStyle buttonStyle = ButtonStyle.Outline)
    {
        _dialog.TertiaryButtonContent = content;
        _dialog.TertiaryButtonStyle = buttonStyle;
        return this;
    }

    /// <summary>
    ///     Sets the cancel button of the dialog.
    /// </summary>
    /// <param name="content">The button content</param>
    /// <param name="callback"></param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="ButtonStyle.Outline" /></param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithCancelButton(
        object content,
        CancelEventHandler? callback = null,
        ButtonStyle buttonStyle = ButtonStyle.Outline)
    {
        _dialog.CancelButtonContent = content;
        _dialog.CancelCallback = callback;
        _dialog.CancelButtonStyle = buttonStyle;
        return this;
    }

    /// <summary>
    ///     Makes the dialog dismissible by clicking outside or pressing escape.
    /// </summary>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder Dismissible()
    {
        _options.Dismissible = true;
        return this;
    }

    /// <summary>
    ///     Sets the maximum width of the dialog.
    /// </summary>
    /// <param name="maxWidth">The maximum width in pixels</param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithMaxWidth(double maxWidth)
    {
        _options.MaxWidth = maxWidth;
        return this;
    }

    /// <summary>
    ///     Sets the minimum width of the dialog.
    /// </summary>
    /// <param name="minWidth">The minimum width in pixels</param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public DialogBuilder WithMinWidth(double minWidth)
    {
        _options.MinWidth = minWidth;
        return this;
    }

    /// <summary>
    ///     Shows the dialog and returns the result.
    /// </summary>
    public Task<DialogResult> ShowAsync(CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<DialogResult>();
        _manager.Show(_dialog, r => tcs.TrySetResult(r), _options);

        if (cancellationToken.CanBeCanceled) cancellationToken.Register(() => Dispatcher.UIThread.InvokeOnDemand(() => _manager.Close(_dialog)));

        return tcs.Task;
    }
}