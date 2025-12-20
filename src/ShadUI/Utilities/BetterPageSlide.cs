using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace ShadUI;

/// <summary>
/// Transitions between two pages by sliding them horizontally or vertically.
/// </summary>
public class BetterPageSlide : IPageTransition
{
    /// <summary>
    /// The axis on which the PageSlide should occur
    /// </summary>
    public enum SlideAxis
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Gets the duration of the animation.
    /// </summary>
    public TimeSpan SlideDuration { get; set; }

    /// <summary>
    /// Gets the duration of the fade animation.
    /// </summary>
    public TimeSpan FadeDuration { get; set; }

    /// <summary>
    /// Gets the orientation of the animation.
    /// </summary>
    public SlideAxis Orientation { get; set; }

    /// <summary>
    /// Gets or sets element easing.
    /// </summary>
    public Easing SlideEasing { get; set; } = new LinearEasing();

    /// <summary>
    /// Gets or sets element fade easing.
    /// </summary>
    public Easing FadeEasing { get; set; } = new LinearEasing();

    /// <inheritdoc />
    public virtual async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var parent = GetVisualParent(from, to);
        var distance = Orientation == SlideAxis.Horizontal ? parent.Bounds.Width : parent.Bounds.Height;
        var translateProperty = Orientation == SlideAxis.Horizontal ? TranslateTransform.XProperty : TranslateTransform.YProperty;

        from?.IsVisible = false;

        if (to != null)
        {
            to.IsVisible = true;
            var translateAnimation = new Animation
            {
                Easing = SlideEasing,
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = translateProperty,
                                Value = forward ? distance * 0.05d : -distance * 0.05d
                            }
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = translateProperty,
                                Value = 0
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = SlideDuration
            };
            var fadeAnimation = new Animation
            {
                Easing = FadeEasing,
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 0.2d
                            }
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 0.2d
                            }
                        },
                        Cue = new Cue(0.2d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 1d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = FadeDuration
            };

            await Task.WhenAll(
                translateAnimation.RunAsync(to, cancellationToken),
                fadeAnimation.RunAsync(to, cancellationToken));
        }
    }

    /// <summary>
    /// Gets the common visual parent of the two control.
    /// </summary>
    /// <param name="from">The from control.</param>
    /// <param name="to">The to control.</param>
    /// <returns>The common parent.</returns>
    /// <exception cref="ArgumentException">
    /// The two controls do not share a common parent.
    /// </exception>
    /// <remarks>
    /// Any one of the parameters may be null, but not both.
    /// </remarks>
    protected static Visual GetVisualParent(Visual? from, Visual? to)
    {
        var p1 = (from ?? to)!.GetVisualParent();
        var p2 = (to ?? from)!.GetVisualParent();

        if (p1 != null && p2 != null && p1 != p2)
        {
            throw new ArgumentException("Controls for PageSlide must have same parent.");
        }

        return p1 ?? throw new InvalidOperationException("Cannot determine visual parent.");
    }
}