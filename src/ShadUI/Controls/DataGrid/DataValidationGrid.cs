using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace ShadUI.Controls;

public class DataValidationGrid : DataGrid
{
    static DataValidationGrid()
    {
        ItemsSourceProperty.OverrideMetadata<DataValidationGrid>(new StyledPropertyMetadata<IEnumerable>(enableDataValidation: true));
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
}