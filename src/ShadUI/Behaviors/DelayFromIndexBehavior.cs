using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace ShadUI;

public class DelayFromIndexBehavior : Behavior<Control>
{
    /// <summary>
    /// Defines the <see cref="Index"/> property.
    /// </summary>
    public static readonly StyledProperty<int> IndexProperty =
        AvaloniaProperty.Register<DelayFromIndexBehavior, int>(nameof(Index));

    /// <summary>
    /// Gets or sets the index used to calculate the delay. The delay will be calculated as Base + Index * Multiplier.
    /// </summary>
    public int Index
    {
        get => GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
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

        if (change.Property == IndexProperty ||
            change.Property == TargetProperty ||
            change.Property == BaseProperty ||
            change.Property == MultiplierProperty)
        {
            ApplyDelay();
        }
    }

    private void ApplyDelay()
    {
        var index = Math.Max(Index, 0);
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