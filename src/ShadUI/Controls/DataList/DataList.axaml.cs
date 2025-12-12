using System.Collections;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace ShadUI.Controls;

[TemplatePart("PART_AddButton", typeof(Button))]
public class DataList : ListBox
{
    /// <summary>
    /// Defines the <see cref="AddCommand"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> AddCommandProperty =
        AvaloniaProperty.Register<DataList, ICommand?>(nameof(AddCommand));

    /// <summary>
    /// Gets or sets the command to execute when the add button is clicked.
    /// </summary>
    public ICommand? AddCommand
    {
        get => GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="RemoveCommand"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> RemoveCommandProperty =
        AvaloniaProperty.Register<DataList, ICommand?>(nameof(RemoveCommand));

    /// <summary>
    /// Gets or sets the command to execute when the remove button is clicked.
    /// Provides the item index to remove as the command parameter.
    /// </summary>
    public ICommand? RemoveCommand
    {
        get => GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    private IDisposable? _addButtonClickSubscription;

    static DataList()
    {
        ItemsSourceProperty.OverrideMetadata<DataList>(new StyledPropertyMetadata<IEnumerable?>(enableDataValidation: true));
    }

    protected override void UpdateDataValidation(
        AvaloniaProperty property,
        BindingValueType state,
        Exception? error)
    {
        if (property == ItemsSourceProperty)
        {
            DataValidationErrors.SetError(this, error);
        }
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new DataListItem();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _addButtonClickSubscription?.Dispose();

        if (e.NameScope.Find<Button>("PART_AddButton") is { } addButton)
        {
            _addButtonClickSubscription = addButton.AddDisposableHandler(Button.ClickEvent, HandleAddButtonClick);
        }
    }

    private void HandleAddButtonClick(object? sender, RoutedEventArgs e)
    {
        if (AddCommand is not { } addCommand) return;
        if (!addCommand.CanExecute(null)) return;
        addCommand.Execute(null);
    }
}