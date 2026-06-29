using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     A modern window with a customizable title bar.
/// </summary>
[TemplatePart("PART_Root", typeof(Panel))]
public class ShadWindow : Window, IRecipient<ThemeChangedMessage>
{
    /// <summary>
    ///     The style key of the window.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(ShadWindow);

    /// <summary>
    ///     The content of the title bar.
    /// </summary>
    public static readonly StyledProperty<object?> TitleBarContentProperty =
        AvaloniaProperty.Register<ShadWindow, object?>(nameof(TitleBarContent));

    /// <summary>
    ///     Gets or sets the value of the <see cref="TitleBarContentProperty" />.
    /// </summary>
    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }

    /// <summary>
    ///     These controls are displayed above all others and fill the entire window.
    ///     Useful for things like popups.
    /// </summary>
    public static readonly StyledProperty<Avalonia.Controls.Controls> HostsProperty =
        AvaloniaProperty.Register<ShadWindow, Avalonia.Controls.Controls>(nameof(Hosts), []);

    /// <summary>
    ///     These controls are displayed above all others and fill the entire window.
    /// </summary>
    public Avalonia.Controls.Controls Hosts
    {
        get => GetValue(HostsProperty);
        set => SetValue(HostsProperty, value);
    }

    /// <summary>
    ///     The thickness of the resize border.
    /// </summary>
    public static readonly StyledProperty<Thickness> ResizeBorderThicknessProperty = AvaloniaProperty.Register<ShadWindow, Thickness>(
        nameof(ResizeBorderThickness),
        defaultValue: new Thickness(4));

    /// <summary>
    ///     Gets or sets the value of the <see cref="ResizeBorderThicknessProperty" />.
    /// </summary>
    public Thickness ResizeBorderThickness
    {
        get => GetValue(ResizeBorderThicknessProperty);
        set => SetValue(ResizeBorderThicknessProperty, value);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ShadWindow" /> class.
    /// </summary>
    protected ShadWindow()
    {
        Hosts = [];
    }

    /// <summary>
    ///     Called when the window is loaded.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        WeakReferenceMessenger.Default.Register(this);

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

        if (desktop.MainWindow is ShadWindow window && window != this) Icon ??= window.Icon;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public void Receive(ThemeChangedMessage message)
    {
        RequestedThemeVariant = message.Variant;
    }
}