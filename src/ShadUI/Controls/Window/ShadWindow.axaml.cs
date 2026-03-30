using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia.Media;
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
    ///     The font size of the title.
    /// </summary>
    public static readonly StyledProperty<double> TitleFontSizeProperty =
        AvaloniaProperty.Register<ShadWindow, double>(nameof(TitleFontSize), 14);

    /// <summary>
    ///     Gets or sets the value of the <see cref="TitleFontSizeProperty" />.
    /// </summary>
    public double TitleFontSize
    {
        get => GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    /// <summary>
    ///     The font weight of the title.
    /// </summary>
    public static readonly StyledProperty<FontWeight> TitleFontWeightProperty =
        AvaloniaProperty.Register<ShadWindow, FontWeight>(nameof(TitleFontWeight), FontWeight.Medium);

    /// <summary>
    ///     Gets or sets the value of the <see cref="TitleFontWeightProperty" />.
    /// </summary>
    public FontWeight TitleFontWeight
    {
        get => GetValue(TitleFontWeightProperty);
        set => SetValue(TitleFontWeightProperty, value);
    }
    
    /// <summary>
    ///     The font size of the version.
    /// </summary>
    public static readonly StyledProperty<double> VersionFontSizeProperty =
        AvaloniaProperty.Register<ShadWindow, double>(nameof(VersionFontSize), 12.8);

    /// <summary>
    ///     Gets or sets the value of the <see cref="TitleFontSizeProperty" />.
    /// </summary>
    public double VersionFontSize
    {
        get => GetValue(VersionFontSizeProperty);
        set => SetValue(VersionFontSizeProperty, value);
    }

    /// <summary>
    ///     The font weight of the version.
    /// </summary>
    public static readonly StyledProperty<FontWeight> VersionFontWeightProperty =
        AvaloniaProperty.Register<ShadWindow, FontWeight>(nameof(VersionFontWeight), FontWeight.Medium);

    /// <summary>
    ///     Gets or sets the value of the <see cref="VersionFontWeightProperty" />.
    /// </summary>
    public FontWeight VersionFontWeight
    {
        get => GetValue(VersionFontWeightProperty);
        set => SetValue(VersionFontWeightProperty, value);
    }

    /// <summary>
    ///    The version text.
    /// </summary>
    public static readonly StyledProperty<string> VersionProperty = AvaloniaProperty.Register<ShadWindow, string>(
        nameof(Version));

    /// <summary>
    ///     Gets or sets the value of the <see cref="VersionProperty" />.
    /// </summary>
    public string Version
    {
        get => GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }

    /// <summary>
    ///     The content of the logo.
    /// </summary>
    public static readonly StyledProperty<Control?> LogoContentProperty =
        AvaloniaProperty.Register<ShadWindow, Control?>(nameof(LogoContent));

    /// <summary>
    ///     Gets or sets the value of the <see cref="LogoContentProperty" />.
    /// </summary>
    public Control? LogoContent
    {
        get => GetValue(LogoContentProperty);
        set => SetValue(LogoContentProperty, value);
    }

    /// <summary>
    ///     The content to override the title bar.
    /// </summary>
    public static readonly StyledProperty<object?> TitleBarContentOverrideProperty =
        AvaloniaProperty.Register<ShadWindow, object?>(nameof(TitleBarContentOverride));

    /// <summary>
    ///     Gets or sets the value of the <see cref="TitleBarContentOverrideProperty" />.
    /// </summary>
    public object? TitleBarContentOverride
    {
        get => GetValue(TitleBarContentOverrideProperty);
        set => SetValue(TitleBarContentOverrideProperty, value);
    }

    /// <summary>
    ///     The controls on the right side of the title bar.
    /// </summary>
    public static readonly StyledProperty<object?> RightWindowTitleBarContentProperty =
        AvaloniaProperty.Register<ShadWindow, object?>(nameof(RightWindowTitleBarContent));

    /// <summary>
    ///     Gets or sets the value of the <see cref="RightWindowTitleBarContentProperty" />.
    /// </summary>
    public object? RightWindowTitleBarContent
    {
        get => GetValue(RightWindowTitleBarContentProperty);
        set => SetValue(RightWindowTitleBarContentProperty, value);
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