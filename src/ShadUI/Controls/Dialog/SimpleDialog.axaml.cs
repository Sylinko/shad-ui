using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

// ReSharper disable once CheckNamespace
namespace ShadUI;

[TemplatePart("PART_PrimaryButton", typeof(Button))]
[TemplatePart("PART_SecondaryButton", typeof(Button))]
[TemplatePart("PART_TertiaryButton", typeof(Button))]
[TemplatePart("PART_CancelButton", typeof(Button))]
internal class SimpleDialog : TemplatedControl
{
    private readonly DialogManager? _manager;

    public SimpleDialog()
    {
    }

    public SimpleDialog(DialogManager manager)
    {
        _manager = manager;
    }

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

    public static readonly StyledProperty<DialogButtonStyle> PrimaryButtonStyleProperty =
        AvaloniaProperty.Register<SimpleDialog, DialogButtonStyle>(nameof(PrimaryButtonStyle));

    public DialogButtonStyle PrimaryButtonStyle
    {
        get => GetValue(PrimaryButtonStyleProperty);
        set => SetValue(PrimaryButtonStyleProperty, value);
    }

    public Action? PrimaryCallback { get; set; }

    public static readonly StyledProperty<object?> SecondaryButtonContentProperty =
        AvaloniaProperty.Register<SimpleDialog, object?>(nameof(SecondaryButtonContent));

    public object? SecondaryButtonContent
    {
        get => GetValue(SecondaryButtonContentProperty);
        set => SetValue(SecondaryButtonContentProperty, value);
    }

    public static readonly StyledProperty<DialogButtonStyle> SecondaryButtonStyleProperty =
        AvaloniaProperty.Register<SimpleDialog, DialogButtonStyle>(nameof(SecondaryButtonStyle),
            DialogButtonStyle.Secondary);

    public DialogButtonStyle SecondaryButtonStyle
    {
        get => GetValue(SecondaryButtonStyleProperty);
        set => SetValue(SecondaryButtonStyleProperty, value);
    }

    public Action? SecondaryCallback { get; set; }

    public static readonly StyledProperty<object?> TertiaryButtonContentProperty =
        AvaloniaProperty.Register<SimpleDialog, object?>(nameof(TertiaryButtonContent));

    public object? TertiaryButtonContent
    {
        get => GetValue(TertiaryButtonContentProperty);
        set => SetValue(TertiaryButtonContentProperty, value);
    }

    public static readonly StyledProperty<DialogButtonStyle> TertiaryButtonStyleProperty =
        AvaloniaProperty.Register<SimpleDialog, DialogButtonStyle>(nameof(TertiaryButtonStyle),
            DialogButtonStyle.Outline);

    public DialogButtonStyle TertiaryButtonStyle
    {
        get => GetValue(TertiaryButtonStyleProperty);
        set => SetValue(TertiaryButtonStyleProperty, value);
    }

    public Action? TertiaryCallback { get; set; }

    public static readonly StyledProperty<object?> CancelButtonContentProperty =
        AvaloniaProperty.Register<SimpleDialog, object?>(nameof(CancelButtonContent));

    public object? CancelButtonContent
    {
        get => GetValue(CancelButtonContentProperty);
        set => SetValue(CancelButtonContentProperty, value);
    }

    public Action? CancelCallback { get; set; }

    public static readonly StyledProperty<DialogButtonStyle> CancelButtonStyleProperty =
        AvaloniaProperty.Register<SimpleDialog, DialogButtonStyle>(nameof(CancelButtonStyle),
            DialogButtonStyle.Outline);

    public DialogButtonStyle CancelButtonStyle
    {
        get => GetValue(CancelButtonStyleProperty);
        set => SetValue(CancelButtonStyleProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        e.NameScope.Get<Button>("PART_PrimaryButton").Click += (_, _) =>
        {
            _manager?.CloseDialog(this, DialogResult.Primary);
            _manager?.OpenLast();
            PrimaryCallback?.Invoke();
        };
        e.NameScope.Get<Button>("PART_SecondaryButton").Click += (_, _) =>
        {
            _manager?.CloseDialog(this, DialogResult.Secondary);
            _manager?.OpenLast();
            SecondaryCallback?.Invoke();
        };
        e.NameScope.Get<Button>("PART_TertiaryButton").Click += (_, _) =>
        {
            _manager?.CloseDialog(this, DialogResult.Tertiary);
            _manager?.OpenLast();
            TertiaryCallback?.Invoke();
        };
        e.NameScope.Get<Button>("PART_CancelButton").Click += (_, _) =>
        {
            _manager?.CloseDialog(this, DialogResult.Cancel);
            _manager?.OpenLast();
            CancelCallback?.Invoke();
        };
    }
}