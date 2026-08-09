using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace ShadUI;

/// <summary>
/// A data template that always returns null, effectively rendering nothing.
/// </summary>
public sealed class EmptyDataTemplate : IDataTemplate
{
    public static EmptyDataTemplate Shared { get; } = new();

    public Control? Build(object? param) => null;

    public bool Match(object? data) => true;
}