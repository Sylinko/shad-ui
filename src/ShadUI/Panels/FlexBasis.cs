// https://github.com/AvaloniaUI/Avalonia.Labs/tree/main/src/Avalonia.Labs.Panels/FlexBasis.cs

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ShadUI;

public readonly struct FlexBasis : IEquatable<FlexBasis>
{
    public double Value { get; }

    public FlexBasisKind Kind { get; }

    public FlexBasis(double value, FlexBasisKind kind = FlexBasisKind.Absolute)
    {
        if (value < 0 || double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException($"Invalid basis value: {value}", nameof(value));
        if (kind is < FlexBasisKind.Auto or > FlexBasisKind.Relative)
            throw new ArgumentException($"Invalid basis kind: {kind}", nameof(kind));
        Value = value;
        Kind = kind;
    }

    public static FlexBasis Auto => new(0.0, FlexBasisKind.Auto);

    public bool IsAuto => Kind == FlexBasisKind.Auto;

    public bool IsAbsolute => Kind == FlexBasisKind.Absolute;

    public bool IsRelative => Kind == FlexBasisKind.Relative;

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public bool Equals(FlexBasis other)
    {
        return IsAuto && other.IsAuto || Value == other.Value && Kind == other.Kind;
    }

    public override bool Equals(object? obj)
    {
        return obj is FlexBasis other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (Value, Kind).GetHashCode();
    }

    public static bool operator ==(FlexBasis left, FlexBasis right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FlexBasis left, FlexBasis right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return Kind switch
        {
            FlexBasisKind.Auto => "Auto",
            FlexBasisKind.Absolute => FormattableString.Invariant($"{Value:G17}"),
            FlexBasisKind.Relative => FormattableString.Invariant($"{Value * 100:G17}%"),
            _ => throw new InvalidOperationException(),
        };
    }

    public static FlexBasis Parse(string str)
    {
        return str.ToUpperInvariant() switch
        {
            "AUTO" => Auto,
            var s when s.EndsWith("%") => new FlexBasis(ParseDouble(s.TrimEnd('%').TrimEnd()) / 100, FlexBasisKind.Relative),
            _ => new FlexBasis(ParseDouble(str), FlexBasisKind.Absolute),
        };

        double ParseDouble(string s)
        {
            return double.Parse(s, CultureInfo.InvariantCulture);
        }
    }
}