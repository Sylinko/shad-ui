// // https://github.com/AvaloniaUI/Avalonia.Labs/tree/main/src/Avalonia.Labs.Panels/Uv.cs

using Avalonia;

namespace ShadUI;

public readonly struct Uv(double u, double v)
{
    public double U { get; } = u;

    public double V { get; } = v;

    public static Uv FromSize(double width, double height, bool swap)
    {
        return new Uv(swap ? height : width, swap ? width : height);
    }

    public static Uv FromSize(Size size, bool swap)
    {
        return FromSize(size.Width, size.Height, swap);
    }

    public static Point ToPoint(Uv uv, bool swap)
    {
        return new Point(swap ? uv.V : uv.U, swap ? uv.U : uv.V);
    }

    public static Size ToSize(Uv uv, bool swap)
    {
        return new Size(swap ? uv.V : uv.U, swap ? uv.U : uv.V);
    }

    public Uv WithU(double u)
    {
        return new Uv(u, V);
    }

    public Uv WithV(double v)
    {
        return new Uv(U, v);
    }

    public override string ToString()
    {
        return $"U: {U}, V: {V}";
    }
}