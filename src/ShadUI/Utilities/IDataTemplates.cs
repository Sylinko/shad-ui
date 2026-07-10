using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace ShadUI;

/// <summary>
/// Represents a collection of data templates that can be used to build controls for different types of data.
/// </summary>
public class IDataTemplates : DataTemplates, IDataTemplate
{
    public Control? Build(object? param) => this.FirstOrDefault(x => x.Match(param))?.Build(param);

    public bool Match(object? data) => this.Any(x => x.Match(data));
}