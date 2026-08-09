using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace ShadUI;

/// <summary>
/// A control template that always throws a NotImplementedException when attempting to build a control, effectively indicating that no control can be built from this template.
/// </summary>
public sealed class EmptyControlTemplate : IControlTemplate
{
    public static EmptyControlTemplate Shared { get; } = new();

    public TemplateResult<Control>? Build(TemplatedControl param) => null;
}