using Avalonia;
using Avalonia.Controls.Primitives;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Represents a loading spinner control.
/// </summary>
public class Loading : TemplatedControl
{
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<Loading, double>(nameof(Size), 24d);

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public Loading()
    {
        this[!WidthProperty] = this[!SizeProperty];
        this[!HeightProperty] = this[!SizeProperty];
    }
}