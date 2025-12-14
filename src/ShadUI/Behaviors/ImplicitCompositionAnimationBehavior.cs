using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;
using Avalonia.Rendering.Composition;
using Avalonia.Xaml.Interactivity;

namespace ShadUI;

/// <summary>
/// A behavior that applies implicit composition animations to a control.
/// </summary>
public class ImplicitCompositionAnimationBehavior : Behavior<Control>
{
    /// <summary>
    /// Defines the <see cref="Animations"/> property.
    /// </summary>
    public static readonly DirectProperty<ImplicitCompositionAnimationBehavior, AvaloniaList<CompositionAnimation>> AnimationsProperty =
        AvaloniaProperty.RegisterDirect<ImplicitCompositionAnimationBehavior, AvaloniaList<CompositionAnimation>>(
            nameof(Animations),
            o => o.Animations);

    /// <summary>
    /// Gets the collection of implicit animations to apply.
    /// </summary>
    [Content]
    public AvaloniaList<CompositionAnimation> Animations => field ??= [];

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.AttachedToVisualTree += OnAttachedToVisualTree;
            UpdateAnimations();
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject != null)
        {
            AssociatedObject.AttachedToVisualTree -= OnAttachedToVisualTree;
        }
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (AssociatedObject == null) return;

        // Get the CompositionVisual for the control
        var visual = ElementComposition.GetElementVisual(AssociatedObject);
        if (visual == null) return;

        var compositor = visual.Compositor;

        // Ensure the ImplicitAnimations collection exists
        visual.ImplicitAnimations ??= compositor.CreateImplicitAnimationCollection();

        foreach (var animationDefinition in Animations)
        {
            if (animationDefinition.Target == CompositionAnimationTarget.None) continue;

            var targetName = animationDefinition.Target.ToString();
            var animation = CompositionUtils.CreateKeyFrameAnimation(compositor, animationDefinition.Target);

            if (animation == null) continue;

            animation.Target = targetName;
            animation.Duration = animationDefinition.Duration;

            if (animationDefinition.Delay > TimeSpan.Zero)
            {
                animation.DelayTime = animationDefinition.Delay;
            }

            // Implicit animations typically animate to the new value ("this.FinalValue")
            if (animationDefinition.Easing is { } easingFunction)
            {
                animation.InsertExpressionKeyFrame(1.0f, "this.FinalValue", easingFunction);
            }
            else
            {
                animation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            }

            // Register the animation in the ImplicitAnimations collection
            visual.ImplicitAnimations[targetName] = animation;
        }
    }
}