using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace ShadUI;

/// <summary>
/// Provides attached properties for binding assistance.
/// </summary>
public static class BindingAssist
{
    /// <summary>
    /// Defines the <see cref="DataTemplates"/> attached property.
    /// </summary>
    /// <remarks>
    /// <see cref="Control.DataTemplates"/> is not settable directly in XAML, so this attached property
    /// allows binding a collection of data templates to a control.
    /// </remarks>
    public static readonly AttachedProperty<IEnumerable<IDataTemplate>> DataTemplatesProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, IEnumerable<IDataTemplate>>("DataTemplates");

    /// <summary>
    /// Sets the data templates for the specified control.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetDataTemplates(Control obj, IEnumerable<IDataTemplate> value) => obj.SetValue(DataTemplatesProperty, value);

    /// <summary>
    /// Gets the data templates for the specified control.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static IEnumerable<IDataTemplate> GetDataTemplates(Control obj) => obj.GetValue(DataTemplatesProperty);

    /// <summary>
    /// Defines the <see cref="Classes"/> attached property.
    /// </summary>
    /// <remarks>
    /// <see cref="Control.Classes"/> is not settable directly in XAML, so this attached property
    /// allows binding a collection of classes to a control.
    /// </remarks>
    public static readonly AttachedProperty<object?> ClassesProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, object?>("Classes");

    /// <summary>
    /// Sets the classes for the specified control.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetClasses(Control obj, object? value) => obj.SetValue(ClassesProperty, value);

    /// <summary>
    /// Gets the classes for the specified control.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static object? GetClasses(Control obj) => obj.GetValue(ClassesProperty);

    /// <summary>
    /// Defines the <see cref="ToolTip"/> attached property.
    /// </summary>
    public static readonly AttachedProperty<object?> ToolTipProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, object?>("ToolTip");

    /// <summary>
    /// Sets the tooltip for the specified control. If the value is a non-empty string, it will be used as the tooltip.
    /// If the value is null or an empty string, the tooltip will be cleared.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetToolTip(Control obj, object? value) => obj.SetValue(ToolTipProperty, value);

    /// <summary>
    /// Gets the tooltip for the specified control.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static object? GetToolTip(Control obj) => obj.GetValue(ToolTipProperty);

    static BindingAssist()
    {
        DataTemplatesProperty.Changed.AddClassHandler<Control>(HandleDataTemplatesChanged);
        ClassesProperty.Changed.AddClassHandler<Control>(HandleClassesChanged);
        ToolTipProperty.Changed.AddClassHandler<Control>(HandleToolTipChanged);
    }

    private static void HandleDataTemplatesChanged(Control sender, AvaloniaPropertyChangedEventArgs args)
    {
        sender.DataTemplates.Clear();
        if (args.NewValue is IEnumerable<IDataTemplate> dataTemplates) sender.DataTemplates.AddRange(dataTemplates);
    }

    private static void HandleClassesChanged(Control sender, AvaloniaPropertyChangedEventArgs args)
    {
        var oldClasses = ConvertClasses(args.OldValue).ToArray();
        if (oldClasses.Length > 0)
        {
            for (var i = sender.Classes.Count - 1; i >= 0; i--)
            {
                if (oldClasses.Contains(sender.Classes[i]))
                {
                    sender.Classes.RemoveAt(i);
                }
            }
        }

        sender.Classes.AddRange(ConvertClasses(args.NewValue));

        static IEnumerable<string> ConvertClasses(object? obj)
        {
            return obj switch
            {
                IEnumerable<string> enumerable => enumerable,
                IEnumerable enumerable => enumerable.Cast<object>().Select(x => x.ToString()).OfType<string>(),
                _ when obj?.ToString() is { Length: > 0 } @string => [@string],
                _ => []
            };
        }
    }

    private static void HandleToolTipChanged(Control sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not string str || !string.IsNullOrWhiteSpace(str))
        {
            ToolTip.SetTip(sender, args.NewValue);
        }
        else
        {
            sender.ClearValue(ToolTip.TipProperty);
        }
    }
}