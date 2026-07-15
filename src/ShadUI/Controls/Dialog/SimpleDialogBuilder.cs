using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Builds a simple dialog.
/// </summary>
public readonly ref struct SimpleDialogBuilder
{
    private readonly DialogHost? _host;
    private readonly SimpleDialog _dialog;
    private readonly DialogOptions _options = new();

    internal SimpleDialogBuilder(DialogHost? host, object content, object? title = null)
    {
        _host = host;
        _dialog = new SimpleDialog
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
    public SimpleDialogBuilder WithTitle(string title)
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
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public SimpleDialogBuilder WithPrimaryButton(
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
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public SimpleDialogBuilder WithSecondaryButton(
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
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public SimpleDialogBuilder WithTertiaryButton(
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
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public SimpleDialogBuilder WithCancelButton(
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
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public SimpleDialogBuilder Dismissible()
    {
        _options.Dismissible = true;
        return this;
    }

    /// <summary>
    ///     Sets the maximum width of the dialog.
    /// </summary>
    /// <param name="maxWidth">The maximum width in pixels</param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public SimpleDialogBuilder WithMaxWidth(double maxWidth)
    {
        _options.MaxWidth = maxWidth;
        return this;
    }

    /// <summary>
    ///     Sets the minimum width of the dialog.
    /// </summary>
    /// <param name="minWidth">The minimum width in pixels</param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public SimpleDialogBuilder WithMinWidth(double minWidth)
    {
        _options.MinWidth = minWidth;
        return this;
    }

    /// <summary>
    ///     Shows the dialog and returns the result.
    /// </summary>
    public Task<DialogResult> ShowAsync(CancellationToken cancellationToken = default)
    {
        return DialogManager.ShowAsync(_host, _dialog, _options, cancellationToken);
    }
}