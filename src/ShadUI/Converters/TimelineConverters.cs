using Avalonia;
using Avalonia.Data.Converters;

namespace ShadUI;

public static class TimelineConverters
{
    public static IValueConverter HalfDouble { get; } = new FuncValueConverter<double, Point>(x => new Point(x / 2d, x / 2d));

    public static IValueConverter HalfRect { get; } = new FuncValueConverter<Rect, Point>(x => new Point(x.Width / 2d, x.Height / 2d));
}