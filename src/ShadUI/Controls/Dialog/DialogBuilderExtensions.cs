using Avalonia.Controls;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Fluent API for building dialogs.
/// </summary>
public static class DialogBuilderExtensions
{
    /// <summary>
    ///     Creates a simple dialog.
    /// </summary>
    /// <param name="manager">The <see cref="DialogManager" /></param>
    /// <param name="title">The dialog title</param>
    /// <param name="message">The dialog message</param>
    /// <returns>A new instance of <see cref="SimpleDialogBuilder" /></returns>
    public static SimpleDialogBuilder CreateDialog(this DialogManager manager, string title, string message)
        => new SimpleDialogBuilder(manager).CreateDialog(title, message);

    /// <summary>
    ///     Sets the primary button of the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <param name="text">The button text</param>
    /// <param name="callback">The method that is called once the button is clicked</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Primary" /></param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder WithPrimaryButton(this SimpleDialogBuilder builder, string text,
        Action? callback = null,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Primary)
    {
        builder.PrimaryButtonText = text;
        builder.PrimaryCallback = callback;
        builder.PrimaryButtonStyle = buttonStyle;
        return builder;
    }

    /// <summary>
    ///     Sets the primary button of the dialog with an asynchronous callback.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <param name="text">The button text</param>
    /// <param name="asyncCallback">The asynchronous method that is called once the button is clicked</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Primary" /></param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder WithPrimaryButton(this SimpleDialogBuilder builder, string text,
        Func<Task>? asyncCallback = null,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Primary)
    {
        builder.PrimaryButtonText = text;
        builder.PrimaryCallbackAsync = asyncCallback;
        builder.PrimaryButtonStyle = buttonStyle;
        return builder;
    }

    /// <summary>
    ///     Sets the secondary button of the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <param name="text">The button text</param>
    /// <param name="callback">The method that is called once the button is clicked</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Secondary" /></param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder WithSecondaryButton(this SimpleDialogBuilder builder, string text,
        Action? callback = null,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Secondary)
    {
        builder.SecondaryButtonText = text;
        builder.SecondaryCallback = callback;
        builder.SecondaryButtonStyle = buttonStyle;
        return builder;
    }

    /// <summary>
    ///     Sets the secondary button of the dialog with an asynchronous callback.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <param name="text">The button text</param>
    /// <param name="asyncCallback">The asynchronous method that is called once the button is clicked</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Secondary" /></param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder WithSecondaryButton(this SimpleDialogBuilder builder, string text,
        Func<Task>? asyncCallback = null,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Secondary)
    {
        builder.SecondaryButtonText = text;
        builder.SecondaryCallbackAsync = asyncCallback;
        builder.SecondaryButtonStyle = buttonStyle;
        return builder;
    }

    /// <summary>
    ///     Sets the tertiary button of the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <param name="text">The button text</param>
    /// <param name="callback">The method that is called once the button is clicked</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Outline" /></param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder WithTertiaryButton(this SimpleDialogBuilder builder, string text,
        Action? callback = null,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Outline)
    {
        builder.TertiaryButtonText = text;
        builder.TertiaryCallback = callback;
        builder.TertiaryButtonStyle = buttonStyle;
        return builder;
    }

    /// <summary>
    ///     Sets the tertiary button of the dialog with an asynchronous callback.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <param name="text">The button text</param>
    /// <param name="asyncCallback">The asynchronous method that is called once the button is clicked</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Outline" /></param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder WithTertiaryButton(this SimpleDialogBuilder builder, string text,
        Func<Task>? asyncCallback = null,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Outline)
    {
        builder.TertiaryButtonText = text;
        builder.TertiaryCallbackAsync = asyncCallback;
        builder.TertiaryButtonStyle = buttonStyle;
        return builder;
    }

    /// <summary>
    ///     Sets the cancel button of the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <param name="text">The button text</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Outline" /></param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder WithCancelButton(this SimpleDialogBuilder builder, string text,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Outline)
    {
        builder.CancelButtonText = text;
        builder.CancelButtonStyle = buttonStyle;
        return builder;
    }

    /// <summary>
    ///     Sets the cancel button of the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <param name="text">The button text</param>
    /// <param name="callback">The method that is called once the button is clicked</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Outline" /></param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder WithCancelButton(this SimpleDialogBuilder builder, string text, Action callback,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Outline)
    {
        builder.CancelButtonText = text;
        builder.CancelCallback = callback;
        builder.CancelButtonStyle = buttonStyle;
        return builder;
    }

    /// <summary>
    ///     Sets the cancel button of the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <param name="text">The button text</param>
    /// <param name="asyncCallback">The asynchronous method that is called once the button is clicked</param>
    /// <param name="buttonStyle">The style of the button. The default is <see cref="DialogButtonStyle.Outline" /></param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder WithCancelButton(this SimpleDialogBuilder builder, string text,
        Func<Task>? asyncCallback,
        DialogButtonStyle buttonStyle = DialogButtonStyle.Outline)
    {
        builder.CancelButtonText = text;
        builder.CancelCallbackAsync = asyncCallback;
        builder.CancelButtonStyle = buttonStyle;
        return builder;
    }

    /// <summary>
    ///     Makes the dialog dismissible by clicking outside or pressing escape.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder Dismissible(this SimpleDialogBuilder builder)
    {
        builder.Options.Dismissible = true;
        return builder;
    }

    /// <summary>
    ///     Sets the maximum width of the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <param name="maxWidth">The maximum width in pixels</param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder WithMaxWidth(this SimpleDialogBuilder builder, double maxWidth)
    {
        builder.Options.MaxWidth = maxWidth;
        return builder;
    }

    /// <summary>
    ///     Sets the minimum width of the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    /// <param name="minWidth">The minimum width in pixels</param>
    /// <returns>The modified <see cref="SimpleDialogBuilder" /> instance</returns>
    public static SimpleDialogBuilder WithMinWidth(this SimpleDialogBuilder builder, double minWidth)
    {
        builder.Options.MinWidth = minWidth;
        return builder;
    }

    /// <summary>
    ///     Shows the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="SimpleDialogBuilder" /></param>
    public static void Show(this SimpleDialogBuilder builder)
    {
        builder.Show();
    }

    /// <summary>
    ///     Creates a dialog with a custom context.
    /// </summary>
    /// <param name="manager">The <see cref="DialogManager" /></param>
    /// <param name="control">The dialog content</param>
    public static DialogBuilder CreateDialog(this DialogManager manager, Control control)
    {
        return new DialogBuilder(manager).CreateDialog(control);
    }

    /// <summary>
    ///     Sets the callback for the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="DialogBuilder" /></param>
    /// <param name="callback">The method that is called when the dialog is closed</param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public static DialogBuilder WithCallback(this DialogBuilder builder, Action<DialogResult> callback)
    {
        builder.Callback = callback;
        return builder;
    }

    /// <summary>
    ///     Makes the dialog dismissible by clicking outside or pressing escape. If set to true, this will take precedence over
    ///     toggling <see cref="DialogManager.PreventDismissal()" />
    /// </summary>
    /// <param name="builder">The <see cref="DialogBuilder" /></param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public static DialogBuilder Dismissible(this DialogBuilder builder)
    {
        builder.Options.Dismissible = true;
        return builder;
    }

    /// <summary>
    ///     Sets the maximum width of the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="DialogBuilder" /></param>
    /// <param name="maxWidth">The maximum width in pixels</param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public static DialogBuilder WithMaxWidth(this DialogBuilder builder, double maxWidth)
    {
        builder.Options.MaxWidth = maxWidth;
        return builder;
    }

    /// <summary>
    ///     Sets the minimum width of the dialog.
    /// </summary>
    /// <param name="builder">The <see cref="DialogBuilder" /></param>
    /// <param name="minWidth">The minimum width in pixels</param>
    /// <returns>The modified <see cref="DialogBuilder" /> instance</returns>
    public static DialogBuilder WithMinWidth(this DialogBuilder builder, double minWidth)
    {
        builder.Options.MinWidth = minWidth;
        return builder;
    }
}