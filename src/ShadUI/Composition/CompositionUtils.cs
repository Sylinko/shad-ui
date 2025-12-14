using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace ShadUI;

/// <summary>
/// Enum defining the target properties for composition animations.
/// </summary>
public enum CompositionAnimationTarget
{
    None,
    Offset,
    Scale,
    CenterPoint,
    RotationAxis,
    Size,
    AnchorPoint,
    Opacity,
    RotationAngle,
    RotationAngleInDegrees
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
            CompositionAnimationTarget.Offset or
                CompositionAnimationTarget.Scale or
                CompositionAnimationTarget.CenterPoint or
                CompositionAnimationTarget.RotationAxis => compositor.CreateVector3KeyFrameAnimation(),
            CompositionAnimationTarget.Size or
                CompositionAnimationTarget.AnchorPoint => compositor.CreateVector2KeyFrameAnimation(),
            CompositionAnimationTarget.Opacity or
                CompositionAnimationTarget.RotationAngle or
                CompositionAnimationTarget.RotationAngleInDegrees =>
                compositor.CreateScalarKeyFrameAnimation(),
            _ => null
        };
    }
}
