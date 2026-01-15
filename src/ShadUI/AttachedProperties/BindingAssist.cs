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
    public static readonly AttachedProperty<IEnumerable<string>> ClassesProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, IEnumerable<string>>("Classes");

    /// <summary>
    /// Sets the classes for the specified control.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetClasses(Control obj, IEnumerable<string> value) => obj.SetValue(ClassesProperty, value);

    /// <summary>
    /// Gets the classes for the specified control.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetClasses(Control obj) => obj.GetValue(ClassesProperty);

    static BindingAssist()
    {
        DataTemplatesProperty.Changed.AddClassHandler<Control>(HandleDataTemplatesChanged);
        ClassesProperty.Changed.AddClassHandler<Control>(HandleClassesChanged);
    }

    private static void HandleDataTemplatesChanged(Control sender, AvaloniaPropertyChangedEventArgs args)
    {
        sender.DataTemplates.Clear();
        if (args.NewValue is IEnumerable<IDataTemplate> dataTemplates) sender.DataTemplates.AddRange(dataTemplates);
    }

    private static void HandleClassesChanged(Control sender, AvaloniaPropertyChangedEventArgs args)
    {
        sender.Classes.Clear();
        if (args.NewValue is IEnumerable<string> classes) sender.Classes.AddRange(classes);
    }
}