using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace ShadUI;

/// <summary>
/// Lays out a button's content with an optional icon or progress indicator.
/// The template supplies the content presenter through Child; adornments are created only when needed.
/// </summary>
public class ButtonContentPresenter : Panel
{
    /// <summary>
    /// Defines the content presenter supplied by the button template.
    /// </summary>
    public static readonly StyledProperty<Control?> ChildProperty =
        Decorator.ChildProperty.AddOwner<ButtonContentPresenter>();

    /// <summary>
    /// Defines the icon displayed while progress is hidden.
    /// </summary>
    public static readonly StyledProperty<object?> IconProperty =
        ButtonAssist.IconProperty.AddOwner<ButtonContentPresenter>();

    /// <summary>
    /// Defines the size shared by the icon and progress indicator.
    /// </summary>
    public static readonly StyledProperty<double> IconSizeProperty =
        ButtonAssist.IconSizeProperty.AddOwner<ButtonContentPresenter>();

    /// <summary>
    /// Defines whether progress replaces the icon.
    /// </summary>
    public static readonly StyledProperty<bool> ShowProgressProperty =
        ButtonAssist.ShowProgressProperty.AddOwner<ButtonContentPresenter>();

    /// <summary>
    /// Defines the gap between the leading adornment and visible content.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<ButtonContentPresenter, double>(nameof(Spacing));

    /// <summary>
    /// Gets or sets the content presenter supplied by the button template. Its identity is independent of adornment changes.
    /// </summary>
    public Control? Child
    {
        get => GetValue(ChildProperty);
        set => SetValue(ChildProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon displayed while progress is hidden.
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the size shared by the icon and progress indicator.
    /// </summary>
    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether progress replaces the icon. The button's content remains mounted.
    /// </summary>
    public bool ShowProgress
    {
        get => GetValue(ShowProgressProperty);
        set => SetValue(ShowProgressProperty, value);
    }

    /// <summary>
    /// Gets or sets the gap between the leading adornment and visible content.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    private double AdornmentProgress => GetValue(AdornmentProgressProperty);

    private static readonly StyledProperty<double> AdornmentProgressProperty =
        AvaloniaProperty.Register<ButtonContentPresenter, double>(nameof(AdornmentProgress));

    private ContentPresenter? _iconPresenter;
    private Viewbox? _iconViewbox;
    private Loading? _loading;
    private ScaleTransform? _adornmentScale;

    static ButtonContentPresenter()
    {
        AffectsMeasure<ButtonContentPresenter>(IconSizeProperty, SpacingProperty, AdornmentProgressProperty);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ChildProperty)
        {
            if (change.GetOldValue<Control?>() is { } oldChild)
            {
                Children.Remove(oldChild);
            }

            if (Child is { } child)
            {
                Children.Add(child);
            }
        }
        else if (change.Property == IconProperty || change.Property == IconSizeProperty || change.Property == ShowProgressProperty)
        {
            UpdateAdornments();
        }
        else if (change.Property == AdornmentProgressProperty)
        {
            if (_adornmentScale is { } scale)
            {
                scale.ScaleX = AdornmentProgress;
                scale.ScaleY = AdornmentProgress;
            }

            UpdateVisibility();
        }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var adornmentSize = new Size(IconSize, IconSize);
        _iconViewbox?.Measure(adornmentSize);
        _loading?.Measure(adornmentSize);

        var leadingWidth = IconSize * AdornmentProgress;
        var spacing = Child?.IsVisible == true ? Spacing * AdornmentProgress : 0;
        Child?.Measure(new Size(Math.Max(0, availableSize.Width - leadingWidth - spacing), availableSize.Height));
        var contentSize = Child?.DesiredSize ?? default;

        return new Size(leadingWidth + spacing + contentSize.Width, Math.Max(leadingWidth, contentSize.Height));
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        // Adornments keep their nominal layout size. The shared transform scales around the left edge and
        // vertical center, matching the animated slot without overwriting the caller's icon transform.
        var adornmentBounds = new Rect(0, (finalSize.Height - IconSize) / 2, IconSize, IconSize);
        _iconViewbox?.Arrange(adornmentBounds);
        _loading?.Arrange(adornmentBounds);

        var leadingWidth = IconSize * AdornmentProgress;
        var spacing = Child?.IsVisible == true ? Spacing * AdornmentProgress : 0;
        var contentX = leadingWidth + spacing;
        Child?.Arrange(new Rect(contentX, 0, Math.Max(0, finalSize.Width - contentX), finalSize.Height));

        return finalSize;
    }

    private void UpdateAdornments()
    {
        if (ShowProgress && _loading is null)
        {
            // Follow the button's current foreground, including checked and hover colors on ToggleButton.
            _loading = new Loading
            {
                RenderTransform = GetAdornmentTransform(),
                RenderTransformOrigin = new RelativePoint(0, 0.5, RelativeUnit.Relative),
                [!TemplatedControl.ForegroundProperty] = this[!TextElement.ForegroundProperty]
            };
            Children.Add(_loading);
        }

        if (!ShowProgress && Icon is not null && _iconViewbox is null)
        {
            _iconPresenter = new ContentPresenter { Name = "Icon" };
            _iconViewbox = new Viewbox
            {
                Child = _iconPresenter,
                RenderTransform = GetAdornmentTransform(),
                RenderTransformOrigin = new RelativePoint(0, 0.5, RelativeUnit.Relative)
            };
            Children.Add(_iconViewbox);
        }

        _iconPresenter?.SetValue(ContentPresenter.ContentProperty, Icon);

        if (_iconViewbox is { } iconViewbox)
        {
            iconViewbox.Width = IconSize;
            iconViewbox.Height = IconSize;
        }

        if (_loading is { } loading)
        {
            loading.Size = IconSize;
        }

        // Only presence changes the slot width. Icon/progress replacement keeps the content in place.
        SetValue(AdornmentProgressProperty, ShowProgress || Icon is not null ? 1 : 0);
        UpdateVisibility();
    }

    private ScaleTransform GetAdornmentTransform()
    {
        if (_adornmentScale is not null)
        {
            return _adornmentScale;
        }

        _adornmentScale = new ScaleTransform(AdornmentProgress, AdornmentProgress);
        Transitions =
        [
            new DoubleTransition
            {
                Property = AdornmentProgressProperty,
                Duration = TimeSpan.FromMilliseconds(100),
                Easing = new CubicEaseInOut()
            }
        ];
        return _adornmentScale;
    }

    private void UpdateVisibility()
    {
        if (_iconViewbox is { } iconViewbox)
        {
            iconViewbox.IsVisible = !ShowProgress && Icon is not null;
        }

        if (_loading is { } loading)
        {
            // Keep outgoing progress visible until its slot has collapsed. Reuse IsVisible rather than
            // retaining a separate outgoing state; a new icon or another busy change replaces it immediately.
            // Avalonia pauses the spinner's style animation when it is no longer effectively visible.
            loading.IsVisible = ShowProgress || loading.IsVisible && Icon is null && AdornmentProgress > 0;
        }
    }
}