using Avalonia;
using Avalonia.Animation.Easings;

namespace ShadUI;

/// <summary>
/// Defines a composition animation configuration.
/// </summary>
public class CompositionAnimation : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="Target"/> property.
    /// </summary>
    public static readonly StyledProperty<CompositionAnimationTarget> TargetProperty =
        AvaloniaProperty.Register<CompositionAnimation, CompositionAnimationTarget>(nameof(Target));

    /// <summary>
    /// Gets or sets the target property to animate.
    /// </summary>
    public CompositionAnimationTarget Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Duration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<CompositionAnimation, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(200));

    /// <summary>
    /// Gets or sets the duration of the animation.
    /// </summary>
    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Delay"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> DelayProperty =
        AvaloniaProperty.Register<CompositionAnimation, TimeSpan>(nameof(Delay));

    /// <summary>
    /// Gets or sets the delay before the animation starts.
    /// </summary>
    public TimeSpan Delay
    {
        get => GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Easing"/> property.
    /// </summary>
    public static readonly StyledProperty<Easing?> EasingProperty =
        AvaloniaProperty.Register<CompositionAnimation, Easing?>(nameof(Easing));

    /// <summary>
    /// Gets or sets the easing function to use.
    /// </summary>
    public Easing? Easing
    {
        get => GetValue(EasingProperty);
        set => SetValue(EasingProperty, value);
    }
}
