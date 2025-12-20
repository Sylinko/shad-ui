using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;
using Avalonia.Reactive;
using Avalonia.Xaml.Interactions.Responsive;
using Avalonia.Xaml.Interactivity;

namespace ShadUI;

public enum AdaptiveRatioSizeMode
{
    Bounds,
    DesiredSize
}

/// <summary>
/// Observes <see cref="NumeratorControl"/> and <see cref="DenominatorControl"/> <see cref="Visual.Bounds"/> property changes 
/// and if triggered sets or removes style classes when conditions from <see cref="AdaptiveClassSetter"/> are met based on the ratio.
/// </summary>
public class AdaptiveRatioBehavior : StyledElementBehavior<Control>
{
    private IDisposable? _numeratorDisposable;
    private IDisposable? _denominatorDisposable;

    /// <summary>
    /// Identifies the <seealso cref="NumeratorControl"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<Control?> NumeratorControlProperty =
        AvaloniaProperty.Register<AdaptiveRatioBehavior, Control?>(nameof(NumeratorControl));

    /// <summary>
    /// Identifies the <seealso cref="NumeratorControlMode"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<AdaptiveRatioSizeMode> NumeratorControlModeProperty =
        AvaloniaProperty.Register<AdaptiveRatioBehavior, AdaptiveRatioSizeMode>(nameof(NumeratorControlMode));

    /// <summary>
    /// Identifies the <seealso cref="DenominatorControl"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<Control?> DenominatorControlProperty =
        AvaloniaProperty.Register<AdaptiveRatioBehavior, Control?>(nameof(DenominatorControl));

    /// <summary>
    /// Identifies the <seealso cref="DenominatorControlMode"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<AdaptiveRatioSizeMode> DenominatorControlModeProperty =
        AvaloniaProperty.Register<AdaptiveRatioBehavior, AdaptiveRatioSizeMode>(nameof(DenominatorControlMode));

    /// <summary>
    /// Identifies the <seealso cref="TargetControl"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<Control?> TargetControlProperty =
        AvaloniaProperty.Register<AdaptiveRatioBehavior, Control?>(nameof(TargetControl));

    /// <summary>
    /// Identifies the <seealso cref="Setters"/> avalonia property.
    /// </summary>
    public static readonly DirectProperty<AdaptiveRatioBehavior, AvaloniaList<AdaptiveClassSetter>> SettersProperty =
        AvaloniaProperty.RegisterDirect<AdaptiveRatioBehavior, AvaloniaList<AdaptiveClassSetter>>(nameof(Setters), t => t.Setters);

    /// <summary>
    /// Gets or sets the numerator control. This is an avalonia property.
    /// </summary>
    [ResolveByName]
    public Control? NumeratorControl
    {
        get => GetValue(NumeratorControlProperty);
        set => SetValue(NumeratorControlProperty, value);
    }

    /// <summary>
    /// Gets or sets the numerator control size mode. This is an avalonia property.
    /// </summary>
    public AdaptiveRatioSizeMode NumeratorControlMode
    {
        get => GetValue(NumeratorControlModeProperty);
        set => SetValue(NumeratorControlModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the denominator control. This is an avalonia property.
    /// </summary>
    [ResolveByName]
    public Control? DenominatorControl
    {
        get => GetValue(DenominatorControlProperty);
        set => SetValue(DenominatorControlProperty, value);
    }

    /// <summary>
    /// Gets or sets the denominator control size mode. This is an avalonia property.
    /// </summary>
    public AdaptiveRatioSizeMode DenominatorControlMode
    {
        get => GetValue(DenominatorControlModeProperty);
        set => SetValue(DenominatorControlModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the target control that class name that should be added or removed when triggered, if not set <see cref="StyledElementBehavior{T}.AssociatedObject"/> is used or <see cref="AdaptiveClassSetter.TargetControl"/> from <see cref="AdaptiveClassSetter"/>. This is an avalonia property.
    /// </summary>
    [ResolveByName]
    public Control? TargetControl
    {
        get => GetValue(TargetControlProperty);
        set => SetValue(TargetControlProperty, value);
    }

    /// <summary>
    /// Gets adaptive class setters collection. This is an avalonia property.
    /// </summary>
    [Content]
    public AvaloniaList<AdaptiveClassSetter> Setters => field ??= [];

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();

        StopObserving();
        StartObserving();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree()
    {
        base.OnDetachedFromVisualTree();

        StopObserving();
    }

    private void StartObserving()
    {
        var numerator = NumeratorControl;
        var denominator = DenominatorControl;

        if (numerator is not null && denominator is not null)
        {
            // Initial update
            Update();

            _numeratorDisposable = numerator
                .GetObservable(Visual.BoundsProperty)
                .Subscribe(new AnonymousObserver<Rect>(_ => Update()));
            _denominatorDisposable = denominator
                .GetObservable(Visual.BoundsProperty)
                .Subscribe(new AnonymousObserver<Rect>(_ => Update()));
        }
    }

    private void StopObserving()
    {
        _numeratorDisposable?.Dispose();
        _denominatorDisposable?.Dispose();
    }

    private void Update()
    {
        var numerator = NumeratorControl;
        var denominator = DenominatorControl;

        if (numerator is null || denominator is null)
        {
            return;
        }

        var nBounds = NumeratorControlMode == AdaptiveRatioSizeMode.DesiredSize ? numerator.DesiredSize : numerator.Bounds.Size;
        var dBounds = DenominatorControlMode == AdaptiveRatioSizeMode.DesiredSize ? denominator.DesiredSize : denominator.Bounds.Size;

        // Avoid division by zero
        var widthRatio = dBounds.Width > 0 ? nBounds.Width / dBounds.Width : 0;
        var heightRatio = dBounds.Height > 0 ? nBounds.Height / dBounds.Height : 0;

        var bounds = new Rect(0, 0, widthRatio, heightRatio);

        Execute(Setters, bounds);
    }

    private void Execute(AvaloniaList<AdaptiveClassSetter>? setters, Rect bounds)
    {
        if (setters is null)
        {
            return;
        }

        foreach (var setter in setters)
        {
            var isMinOrMaxWidthSet = setter.IsSet(AdaptiveClassSetter.MinWidthProperty)
                || setter.IsSet(AdaptiveClassSetter.MaxWidthProperty);
            var widthConditionTriggered = GetResult(setter.MinWidthOperator, bounds.Width, setter.MinWidth)
                && GetResult(setter.MaxWidthOperator, bounds.Width, setter.MaxWidth);

            var isMinOrMaxHeightSet = setter.IsSet(AdaptiveClassSetter.MinHeightProperty)
                || setter.IsSet(AdaptiveClassSetter.MaxHeightProperty);
            var heightConditionTriggered = GetResult(setter.MinHeightOperator, bounds.Height, setter.MinHeight)
                && GetResult(setter.MaxHeightOperator, bounds.Height, setter.MaxHeight);

            var isAddClassTriggered = isMinOrMaxWidthSet switch
            {
                true when !isMinOrMaxHeightSet => widthConditionTriggered,
                false when isMinOrMaxHeightSet => heightConditionTriggered,
                true when isMinOrMaxHeightSet => widthConditionTriggered && heightConditionTriggered,
                _ => false
            };

            var targetControl = setter.GetValue(AdaptiveClassSetter.TargetControlProperty) is not null ? setter.TargetControl
                : GetValue(TargetControlProperty) is not null ? TargetControl
                : AssociatedObject;

            if (targetControl is not null)
            {
                var className = setter.ClassName;
                var isPseudoClass = setter.IsPseudoClass;

                if (isAddClassTriggered)
                {
                    Add(targetControl, className, isPseudoClass);
                }
                else
                {
                    Remove(targetControl, className, isPseudoClass);
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(targetControl));
            }
        }
    }

    private bool GetResult(ComparisonConditionType comparisonConditionType, double property, double value)
    {
        return comparisonConditionType switch
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            ComparisonConditionType.Equal => property == value,
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            ComparisonConditionType.NotEqual => property != value,
            ComparisonConditionType.LessThan => property < value,
            ComparisonConditionType.LessThanOrEqual => property <= value,
            ComparisonConditionType.GreaterThan => property > value,
            ComparisonConditionType.GreaterThanOrEqual => property >= value,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static void Add(Control targetControl, string? className, bool isPseudoClass)
    {
        if (className is null || string.IsNullOrEmpty(className) || targetControl.Classes.Contains(className))
        {
            return;
        }

        if (isPseudoClass)
        {
            ((IPseudoClasses)targetControl.Classes).Add(className);
        }
        else
        {
            targetControl.Classes.Add(className);
        }
    }

    private static void Remove(Control targetControl, string? className, bool isPseudoClass)
    {
        if (className is null || string.IsNullOrEmpty(className) || !targetControl.Classes.Contains(className))
        {
            return;
        }

        if (isPseudoClass)
        {
            ((IPseudoClasses)targetControl.Classes).Remove(className);
        }
        else
        {
            targetControl.Classes.Remove(className);
        }
    }
}