using Avalonia.Animation;

namespace ShadUI;

public enum StringTransitionType
{
    /// <remarks>
    /// old: AAAAA
    /// new: BBBBBBBB
    /// frames:
    /// BAAAA
    /// BBAAA
    /// BBBAA
    /// BBBBA
    /// BBBBB
    /// BBBBBB
    /// BBBBBBB
    /// BBBBBBBB
    /// </remarks>
    Overwrite,

    /// <remarks>
    /// old: AAAAA
    /// new: BBBBBBBB
    /// frames:
    /// AAAA
    /// AAA
    /// AA
    /// A
    /// B
    /// BB
    /// BBB
    /// BBBB
    /// BBBBB
    /// BBBBBB
    /// BBBBBBB
    /// BBBBBBBB
    /// </remarks>
    OneByOne,
}

/// <summary>
/// A transition that interpolates between two strings.
/// </summary>
public class StringTransition : InterpolatingTransitionBase<string?>
{
    public StringTransitionType Type { get; set; }

    protected override string Interpolate(double progress, string? from, string? to)
    {
        from ??= string.Empty;
        to ??= string.Empty;

        return Type switch
        {
            StringTransitionType.Overwrite => InterpolateOverwrite(progress, from, to),
            StringTransitionType.OneByOne => InterpolateOneByOne(progress, from, to),
            _ => throw new InvalidOperationException()
        };
    }

    private static string InterpolateOverwrite(double progress, string from, string to)
    {
        var maxLength = Math.Max(from.Length, to.Length);
        var splitIndex = (int)Math.Round(progress * maxLength);

        var toPart = to.Length >= splitIndex ? to[..splitIndex] : to;
        var fromPart = from.Length > splitIndex ? from[splitIndex..] : string.Empty;

        return toPart + fromPart;
    }

    private static string InterpolateOneByOne(double progress, string from, string to)
    {
        var totalLength = from.Length + to.Length;
        if (totalLength == 0)
        {
            return string.Empty;
        }

        var currentStep = progress * totalLength;
        if (currentStep < from.Length)
        {
            var length = (int)Math.Round(from.Length - currentStep);
            return from[..length];
        }
        else
        {
            var length = (int)Math.Round(currentStep - from.Length);
            if (length > to.Length)
            {
                length = to.Length;
            }
            return to[..length];
        }
    }
}