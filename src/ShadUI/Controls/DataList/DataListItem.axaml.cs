using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace ShadUI.Controls;

[TemplatePart("PART_RemoveButton", typeof(Button))]
public class DataListItem : ListBoxItem
{
    private IDisposable? _removeButtonClickSubscription;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _removeButtonClickSubscription?.Dispose();

        if (e.NameScope.Find<Button>("PART_RemoveButton") is { } removeButton)
        {
            _removeButtonClickSubscription = removeButton.AddDisposableHandler(Button.ClickEvent, HandleAddButtonClick);
        }
    }

    private void HandleAddButtonClick(object? sender, RoutedEventArgs e)
    {
        if (ItemsControl.ItemsControlFromItemContainer(this) is not DataList { RemoveCommand: { } removeCommand } dataList) return;
        var index = dataList.IndexFromContainer(this);
        if (removeCommand.CanExecute(index)) removeCommand.Execute(index);
    }
}