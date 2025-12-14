using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Avalonia.Xaml.Interactivity;

namespace ShadUI;

/// <summary>
/// An action that triggers a composition animation on a control.
/// </summary>
public class CompositionAnimationAction : AvaloniaObject, IAction
{
    /// <summary>
    /// Defines the <see cref="Target"/> property.
    /// </summary>
    public static readonly StyledProperty<CompositionAnimationTarget> TargetProperty =
        AvaloniaProperty.Register<CompositionAnimationAction, CompositionAnimationTarget>(nameof(Target));

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
        AvaloniaProperty.Register<CompositionAnimationAction, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(200));

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
        AvaloniaProperty.Register<CompositionAnimationAction, TimeSpan>(nameof(Delay));

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
    public static readonly StyledProperty<IEasing?> EasingProperty =
        AvaloniaProperty.Register<CompositionAnimationAction, IEasing?>(nameof(Easing));

    /// <summary>
    /// Gets or sets the easing function to use.
    /// </summary>
    public IEasing? Easing
    {
        get => GetValue(EasingProperty);
        set => SetValue(EasingProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="StartValue"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> StartValueProperty =
        AvaloniaProperty.Register<CompositionAnimationAction, object?>(nameof(StartValue));

    /// <summary>
    /// Gets or sets the starting value of the animation. If null, the current value is used.
    /// </summary>
    public object? StartValue
    {
        get => GetValue(StartValueProperty);
        set => SetValue(StartValueProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="EndValue"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> EndValueProperty =
        AvaloniaProperty.Register<CompositionAnimationAction, object?>(nameof(EndValue));

    /// <summary>
    /// Gets or sets the ending value of the animation.
    /// </summary>
    public object? EndValue
    {
        get => GetValue(EndValueProperty);
        set => SetValue(EndValueProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="TargetObject"/> property.
    /// </summary>
    public static readonly StyledProperty<Control?> TargetObjectProperty =
        AvaloniaProperty.Register<CompositionAnimationAction, Control?>(nameof(TargetObject));

    /// <summary>
    /// Gets or sets the target control to animate. If null, the sender is used.
    /// </summary>
    public Control? TargetObject
    {
        get => GetValue(TargetObjectProperty);
        set => SetValue(TargetObjectProperty, value);
    }

    public object Execute(object? sender, object? parameter)
    {
        var control = TargetObject ?? sender as Control;
        if (control == null) return false;

        if (Target == CompositionAnimationTarget.None) return false;

        var visual = ElementComposition.GetElementVisual(control);
        if (visual == null) return false;

        var compositor = visual.Compositor;
        var targetName = Target.ToString();
        var animation = CompositionUtils.CreateKeyFrameAnimation(compositor, Target);
        if (animation == null) return false;

        switch (Target)
        {
            case CompositionAnimationTarget.Offset:
            case CompositionAnimationTarget.Scale:
            case CompositionAnimationTarget.CenterPoint:
            case CompositionAnimationTarget.RotationAxis:
                ConfigureVector3Animation(animation as Vector3KeyFrameAnimation);
                break;

            case CompositionAnimationTarget.Size:
            case CompositionAnimationTarget.AnchorPoint:
                ConfigureVector2Animation(animation as Vector2KeyFrameAnimation);
                break;

            case CompositionAnimationTarget.Opacity:
            case CompositionAnimationTarget.RotationAngle:
            case CompositionAnimationTarget.RotationAngleInDegrees:
                ConfigureScalarAnimation(animation as ScalarKeyFrameAnimation);
                break;
        }

        animation.Target = targetName;
        animation.Duration = Duration;
        if (Delay > TimeSpan.Zero)
        {
            animation.DelayTime = Delay;
        }

        visual.StartAnimation(targetName, animation);
        return true;

    }

    private void ConfigureVector3Animation(Vector3KeyFrameAnimation? animation)
    {
        if (animation == null) return;

        var easing = Easing;

        if (StartValue != null && TryParseVector3(StartValue, out var start))
        {
            if (easing is not null)
            {
                animation.InsertKeyFrame(0f, start, easing);
            }
            else
            {
                animation.InsertKeyFrame(0f, start);
            }
        }

        if (EndValue != null && TryParseVector3(EndValue, out var end))
        {
            if (easing is not null)
            {
                animation.InsertKeyFrame(1f, end, easing);
            }
            else
            {
                animation.InsertKeyFrame(1f, end);
            }
        }
    }

    private void ConfigureVector2Animation(Vector2KeyFrameAnimation? animation)
    {
        if (animation == null) return;

        var easing = Easing;

        if (StartValue != null && TryParseVector2(StartValue, out var start))
        {
            if (easing is not null)
            {
                animation.InsertKeyFrame(0f, start, easing);
            }
            else
            {
                animation.InsertKeyFrame(0f, start);
            }
        }

        if (EndValue != null && TryParseVector2(EndValue, out var end))
        {
            if (easing is not null)
            {
                animation.InsertKeyFrame(1f, end, easing);
            }
            else
            {
                animation.InsertKeyFrame(1f, end);
            }
        }
    }

    private void ConfigureScalarAnimation(ScalarKeyFrameAnimation? animation)
    {
        if (animation == null) return;

        var easing = Easing;

        if (StartValue != null && TryParseFloat(StartValue, out var start))
        {
            if (easing is not null)
            {
                animation.InsertKeyFrame(0f, start, easing);
            }
            else
            {
                animation.InsertKeyFrame(0f, start);
            }
        }

        if (EndValue != null && TryParseFloat(EndValue, out var end))
        {
            if (easing is not null)
            {
                animation.InsertKeyFrame(1f, end, easing);
            }
            else
            {
                animation.InsertKeyFrame(1f, end);
            }
        }
    }

    private static bool TryParseVector3(object value, out Vector3 result)
    {
        result = default;
        switch (value)
        {
            case Vector3 v:
                result = v;
                return true;
            case string s:
            {
                var parts = s.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
                switch (parts.Length)
                {
                    case 3 when
                        float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var x) &&
                        float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var y) &&
                        float.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var z):
                        result = new Vector3(x, y, z);
                        return true;
                    // Allow single value for uniform scaling if needed, but Vector3 usually needs 3.
                    // Or maybe 1 value -> x=y=z? Let's support that for convenience (e.g. Scale).
                    case 1 when float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var uniform):
                        result = new Vector3(uniform, uniform, uniform);
                        return true;
                }
                break;
            }
        }
        return false;
    }

    private static bool TryParseVector2(object value, out Vector2 result)
    {
        result = default;
        if (value is Vector2 v)
        {
            result = v;
            return true;
        }
        if (value is not string s) return false;

        var parts = s.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
        switch (parts.Length)
        {
            case 2 when
                float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var y):
                result = new Vector2(x, y);
                return true;
            case 1 when float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var uniform):
                result = new Vector2(uniform, uniform);
                return true;
            default:
                return false;
        }
    }

    private static bool TryParseFloat(object value, out float result)
    {
        result = 0;
        switch (value)
        {
            case float f:
                result = f;
                return true;
            case double d:
                result = (float)d;
                return true;
            case int i:
                result = i;
                return true;
            case string s when float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                result = parsed;
                return true;
            default:
                return false;
        }
    }
}
