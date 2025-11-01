using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ShadUI.Controls;

/// <summary>
/// In normal use cases, this control behaves like a TextBlock.
/// However, it can enter edit mode by setting IsEditing to true,
/// allowing the user to modify its text content.
/// Currently, this control behaves like a TextBox.
/// </summary>
[TemplatePart(Name = TextBoxPartName, Type = typeof(TextBox), IsRequired = true)]
[TemplatePart(Name = TextBlockPartName, Type = typeof(TextBlock), IsRequired = true)]
public class EditableTextBlock : TemplatedControl
{
    private const string TextBoxPartName = "PART_TextBox";
    private const string TextBlockPartName = "PART_TextBlock";

    /// <summary>
    /// Identifies the Text property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        TextBox.TextProperty.AddOwner<EditableTextBlock>();

    /// <summary>
    /// Identifies the TextWrapping property.
    /// </summary>
    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        TextBox.TextWrappingProperty.AddOwner<EditableTextBlock>();

    /// <summary>
    /// Identifies the TextAlignment property.
    /// </summary>
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        TextBox.TextAlignmentProperty.AddOwner<EditableTextBlock>();

    /// <summary>
    /// Identifies the Watermark property.
    /// </summary>
    public static readonly StyledProperty<string?> WatermarkProperty =
        TextBox.WatermarkProperty.AddOwner<EditableTextBlock>();

    /// <summary>
    /// Indicates whether the control is in editing mode.
    /// </summary>
    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<EditableTextBlock, bool>(nameof(IsEditing));

    /// <summary>
    /// Gets or sets the text content of the control.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets how the text should wrap.
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    /// <summary>
    /// Gets or sets the alignment of the text.
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the watermark text displayed when the control is empty.
    /// </summary>
    public string? Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control is in editing mode.
    /// </summary>
    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    private TextBox? _textBox;
    private IDisposable? _textBoxLostFocusSubscription;
    private IDisposable? _textBoxKeyDownSubscription;
    private TextBlock? _textBlock;

    static EditableTextBlock()
    {
        IsEditingProperty.Changed.AddClassHandler<EditableTextBlock>(HandleIsEditingChanged);
    }

    private static void HandleIsEditingChanged(EditableTextBlock sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (sender._textBox is null || sender._textBlock is null)
        {
            return;
        }

        if (args.NewValue is true)
        {
            sender._textBox.Focus();
            sender._textBox.SelectAll();
        }
    }

    public void EnterEditMode()
    {
        IsEditing = true;
    }

    public void ExitEditMode()
    {
        IsEditing = false;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _textBoxLostFocusSubscription?.Dispose();
        _textBoxKeyDownSubscription?.Dispose();

        _textBox = e.NameScope.Get<TextBox>(TextBoxPartName);
        _textBlock = e.NameScope.Get<TextBlock>(TextBlockPartName);

        _textBoxLostFocusSubscription = _textBox.AddDisposableHandler(LostFocusEvent, (_, _) => IsEditing = false);
        _textBoxKeyDownSubscription = _textBox.AddDisposableHandler(KeyDownEvent, (_, ev) =>
        {
            if (ev.Key is Key.Enter or Key.Escape)
            {
                IsEditing = false;
                ev.Handled = true;
            }
        });
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsEditing && e is { ClickCount: 2, Pointer.IsPrimary: true })
        {
            IsEditing = true;
            e.Handled = true;
        }

        base.OnPointerPressed(e);
    }
}