using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Converts the scroll value, minimum, and maximum of a scrollbar to an opacity mask.
/// </summary>
/// <remarks>
///     This converter is used to create vertical or horizontal gradient opacity masks for scrollable content areas.
///     It fades the edges based on whether the content can be scrolled in that direction.
/// </remarks>
public class ScrollerToOpacityMask : IMultiValueConverter
{
    private readonly Func<double, double, double, IBrush> _func;

    private static readonly IBrush NoneFadedVertical = CreateVerticalBrush(Colors.Black, Colors.Black);
    private static readonly IBrush BottomFadedVertical = CreateVerticalBrush(Colors.Black, Colors.Transparent);
    private static readonly IBrush TopFadedVertical = CreateVerticalBrush(Colors.Transparent, Colors.Black);
    private static readonly IBrush BothFadedVertical = CreateVerticalBrush(Colors.Transparent, Colors.Transparent);

    private static readonly IBrush NoneFadedHorizontal = CreateHorizontalBrush(Colors.Black, Colors.Black);
    private static readonly IBrush RightFadedHorizontal = CreateHorizontalBrush(Colors.Black, Colors.Transparent);
    private static readonly IBrush LeftFadedHorizontal = CreateHorizontalBrush(Colors.Transparent, Colors.Black);
    private static readonly IBrush BothFadedHorizontal = CreateHorizontalBrush(Colors.Transparent, Colors.Transparent);

    /// <summary>
    ///     Gets the vertical mask instance for creating fade-out effects at the top and bottom of scrollable content.
    /// </summary>
    public static ScrollerToOpacityMask Vertical { get; } = new((value, min, max) =>
    {
        var canScrollUp = value > min;
        var canScrollDown = value < max;

        if (canScrollUp && canScrollDown) return BothFadedVertical;
        if (canScrollUp) return TopFadedVertical;
        if (canScrollDown) return BottomFadedVertical;

        return NoneFadedVertical;
    });

    /// <summary>
    ///     Gets the horizontal mask instance for creating fade-out effects at the left and right of scrollable content.
    /// </summary>
    public static ScrollerToOpacityMask Horizontal { get; } = new((value, min, max) =>
    {
        var canScrollLeft = value > min;
        var canScrollRight = value < max;

        if (canScrollLeft && canScrollRight) return BothFadedHorizontal;
        if (canScrollLeft) return LeftFadedHorizontal;
        if (canScrollRight) return RightFadedHorizontal;

        return NoneFadedHorizontal;
    });

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScrollerToOpacityMask" /> class.
    /// </summary>
    private ScrollerToOpacityMask(Func<double, double, double, IBrush> func)
    {
        _func = func;
    }

    /// <summary>
    ///     Converts the value of the scroller to an opacity mask.
    /// </summary>
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 3) return null;
        if (values[0] is not double val) return null;
        if (values[1] is not double min) return null;
        if (values[2] is not double max) return null;

        return _func(val, min, max);
    }

    /// <summary>
    ///     Helper method to create a linear gradient brush for vertical scrolling.
    /// </summary>
    private static LinearGradientBrush CreateVerticalBrush(Color topColor, Color bottomColor)
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0.0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1.0, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(topColor, 0.0),
                new GradientStop(Colors.Black, 0.05),
                new GradientStop(Colors.Black, 0.95),
                new GradientStop(bottomColor, 1.0)
            ]
        };
    }

    /// <summary>
    ///     Helper method to create a linear gradient brush for horizontal scrolling.
    /// </summary>
    private static LinearGradientBrush CreateHorizontalBrush(Color leftColor, Color rightColor)
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.0, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1.0, 0.5, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(leftColor, 0.0),
                new GradientStop(Colors.Black, 0.05),
                new GradientStop(Colors.Black, 0.95),
                new GradientStop(rightColor, 1.0)
            ]
        };
    }
}