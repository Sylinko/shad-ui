using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace ShadUI;

public class DelayFromIndexBehavior : Behavior<Control>
{
    /// <summary>
    /// Defines the <see cref="Self"/> property.
    /// </summary>
    public static readonly StyledProperty<Control?> SelfProperty =
        AvaloniaProperty.Register<DelayFromIndexBehavior, Control?>(nameof(Self));

    /// <summary>
    /// Gets or sets the control to apply the delay to. If null, uses the AssociatedObject.
    /// </summary>
    public Control? Self
    {
        get => GetValue(SelfProperty);
        set => SetValue(SelfProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Target"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> TargetProperty =
        AvaloniaProperty.Register<DelayFromIndexBehavior, object?>(nameof(Target));

    /// <summary>
    /// Gets or sets the target for setting Delay. Can be <see cref="TransitionBase"/> or <see cref="Animation"/>
    /// </summary>
    public object? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Base"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> BaseProperty =
        AvaloniaProperty.Register<DelayFromIndexBehavior, TimeSpan>(nameof(Base));

    /// <summary>
    /// Gets or sets the base time span for calculating the delay.
    /// </summary>
    public TimeSpan Base
    {
        get => GetValue(BaseProperty);
        set => SetValue(BaseProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Multiplier"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> MultiplierProperty =
        AvaloniaProperty.Register<DelayFromIndexBehavior, TimeSpan>(nameof(Multiplier), TimeSpan.FromSeconds(0.1));

    /// <summary>
    /// Gets or sets the multiplier time span for calculating the delay.
    /// </summary>
    public TimeSpan Multiplier
    {
        get => GetValue(MultiplierProperty);
        set => SetValue(MultiplierProperty, value);
    }

    protected override void OnAttachedToVisualTree()
    {
        ApplyDelay();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelfProperty ||
            change.Property == TargetProperty ||
            change.Property == BaseProperty ||
            change.Property == MultiplierProperty)
        {
            ApplyDelay();
        }
    }

    private void ApplyDelay()
    {
        if ((Self ?? AssociatedObject) is not { } control) return;

        var index = -1;
        if (ItemsControl.ItemsControlFromItemContainer(control) is { } itemContainer)
        {
            // get the index of the control within the ItemsControl
            index = itemContainer.IndexFromContainer(control);
        }
        else if (control.GetVisualParent() is { } parent)
        {
            switch (parent)
            {
                case Panel panel:
                {
                    index = panel.Children.IndexOf(control);
                    break;
                }
                default:
                {
                    index = parent.GetVisualChildren().TakeWhile(child => child != control).Count();
                    break;
                }
            }
        }

        if (index < 0) return;

        var delay = TimeSpan.FromMilliseconds(Base.TotalMilliseconds + index * Multiplier.TotalMilliseconds);

        switch (Target)
        {
            case TransitionBase transition:
                transition.Delay = delay;
                break;
            case Animation animation:
                animation.Delay = delay;
                break;
        }
    }
}