using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace ShadUI;

/// <summary>
/// Enum defining the target properties for composition animations.
/// </summary>
public enum CompositionAnimationTarget
{
    None,

    // CompositionVisual.generated.cs properties
    Visible,
    Opacity,
    ClipToBounds,
    Offset,
    Size,
    AnchorPoint,
    CenterPoint,
    RotationAngle,
    Orientation,
    Scale,

    // CompositionSolidColorVisual.generated.cs properties
    Color,
}

/// <summary>
/// Helper methods for composition animations.
/// </summary>
public static class CompositionUtils
{
    /// <summary>
    /// Creates a KeyFrameAnimation based on the target property.
    /// </summary>
    public static KeyFrameAnimation? CreateKeyFrameAnimation(Compositor compositor, CompositionAnimationTarget target)
    {
        return target switch
        {
            CompositionAnimationTarget.Visible or
                CompositionAnimationTarget.ClipToBounds => compositor.CreateBooleanKeyFrameAnimation(),

            CompositionAnimationTarget.Opacity or
                CompositionAnimationTarget.RotationAngle => compositor.CreateScalarKeyFrameAnimation(),

            CompositionAnimationTarget.Size or
                CompositionAnimationTarget.AnchorPoint => compositor.CreateVector2KeyFrameAnimation(),

            CompositionAnimationTarget.Offset or
                CompositionAnimationTarget.CenterPoint or
                CompositionAnimationTarget.Scale => compositor.CreateVector3KeyFrameAnimation(),

            CompositionAnimationTarget.Orientation => compositor.CreateQuaternionKeyFrameAnimation(),

            CompositionAnimationTarget.Color => compositor.CreateColorKeyFrameAnimation(),

            _ => null
        };
    }
}
