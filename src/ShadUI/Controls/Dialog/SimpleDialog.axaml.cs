using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

// ReSharper disable once CheckNamespace
namespace ShadUI;

[TemplatePart("PART_PrimaryButton", typeof(Button))]
[TemplatePart("PART_SecondaryButton", typeof(Button))]
[TemplatePart("PART_TertiaryButton", typeof(Button))]
[TemplatePart("PART_CancelButton", typeof(Button))]
internal class SimpleDialog : TemplatedControl
{
    /// <summary>
    ///     Gets or sets the close callback installed by the owning <see cref="DialogHost"/> while
    ///     this control is part of that host's stack.
    /// </summary>
    public Action<DialogResult>? CloseRequested { get; set; }

    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<SimpleDialog, object?>(nameof(Title));

    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<SimpleDialog, object?>(nameof(Content));

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly StyledProperty<object?> PrimaryButtonContentProperty =
        AvaloniaProperty.Register<SimpleDialog, object?>(nameof(PrimaryButtonContent));

    public object? PrimaryButtonContent
    {
        get => GetValue(PrimaryButtonContentProperty);
        set => SetValue(PrimaryButtonContentProperty, value);
    }

    public static readonly StyledProperty<ButtonStyle> PrimaryButtonStyleProperty =
        AvaloniaProperty.Register<SimpleDialog, ButtonStyle>(nameof(PrimaryButtonStyle));

    public ButtonStyle PrimaryButtonStyle
    {
        get => GetValue(PrimaryButtonStyleProperty);
        set => SetValue(PrimaryButtonStyleProperty, value);
    }

    public CancelEventHandler? PrimaryCallback { get; set; }

    public static readonly StyledProperty<object?> SecondaryButtonContentProperty =
        AvaloniaProperty.Register<SimpleDialog, object?>(nameof(SecondaryButtonContent));

    public object? SecondaryButtonContent
    {
        get => GetValue(SecondaryButtonContentProperty);
        set => SetValue(SecondaryButtonContentProperty, value);
    }

    public static readonly StyledProperty<ButtonStyle> SecondaryButtonStyleProperty =
        AvaloniaProperty.Register<SimpleDialog, ButtonStyle>(
            nameof(SecondaryButtonStyle),
            ButtonStyle.Secondary);

    public ButtonStyle SecondaryButtonStyle
    {
        get => GetValue(SecondaryButtonStyleProperty);
        set => SetValue(SecondaryButtonStyleProperty, value);
    }

    public CancelEventHandler? SecondaryCallback { get; set; }

    public static readonly StyledProperty<object?> TertiaryButtonContentProperty =
        AvaloniaProperty.Register<SimpleDialog, object?>(nameof(TertiaryButtonContent));

    public object? TertiaryButtonContent
    {
        get => GetValue(TertiaryButtonContentProperty);
        set => SetValue(TertiaryButtonContentProperty, value);
    }

    public static readonly StyledProperty<ButtonStyle> TertiaryButtonStyleProperty =
        AvaloniaProperty.Register<SimpleDialog, ButtonStyle>(
            nameof(TertiaryButtonStyle),
            ButtonStyle.Outline);

    public ButtonStyle TertiaryButtonStyle
    {
        get => GetValue(TertiaryButtonStyleProperty);
        set => SetValue(TertiaryButtonStyleProperty, value);
    }

    public CancelEventHandler? TertiaryCallback { get; set; }

    public static readonly StyledProperty<object?> CancelButtonContentProperty =
        AvaloniaProperty.Register<SimpleDialog, object?>(nameof(CancelButtonContent));

    public object? CancelButtonContent
    {
        get => GetValue(CancelButtonContentProperty);
        set => SetValue(CancelButtonContentProperty, value);
    }

    public CancelEventHandler? CancelCallback { get; set; }

    public static readonly StyledProperty<ButtonStyle> CancelButtonStyleProperty =
        AvaloniaProperty.Register<SimpleDialog, ButtonStyle>(
            nameof(CancelButtonStyle),
            ButtonStyle.Outline);

    public ButtonStyle CancelButtonStyle
    {
        get => GetValue(CancelButtonStyleProperty);
        set => SetValue(CancelButtonStyleProperty, value);
    }

    static SimpleDialog()
    {
        KeyDownEvent.AddClassHandler<SimpleDialog>(
            HandleKeyDownEvent,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble | RoutingStrategies.Direct,
            true);
    }

    private static void HandleKeyDownEvent(SimpleDialog sender, KeyEventArgs args)
    {
        if (args.Key is Key.Escape)
        {
            sender._cancelHandler?.Invoke(sender, args);
        }
    }

    private EventHandler<RoutedEventArgs>? _cancelHandler;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        e.NameScope.Get<Button>("PART_PrimaryButton").Click += CreateClickHandler(() => PrimaryCallback, DialogResult.Primary);
        e.NameScope.Get<Button>("PART_SecondaryButton").Click += CreateClickHandler(() => SecondaryCallback, DialogResult.Secondary);
        e.NameScope.Get<Button>("PART_TertiaryButton").Click += CreateClickHandler(() => TertiaryCallback, DialogResult.Tertiary);
        e.NameScope.Get<Button>("PART_CancelButton").Click += _cancelHandler = CreateClickHandler(() => CancelCallback, DialogResult.Cancel);

        EventHandler<RoutedEventArgs> CreateClickHandler(Func<CancelEventHandler?> callbackFactory, DialogResult result)
        {
            return delegate
            {
                var cancelEventArgs = new CancelEventArgs();
                callbackFactory()?.Invoke(this, cancelEventArgs);
                if (cancelEventArgs.Cancel) return;

                CloseRequested?.Invoke(result);
            };
        }
    }
}