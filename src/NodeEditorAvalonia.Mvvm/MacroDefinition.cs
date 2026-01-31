using System.Windows.Input;

namespace NodeEditor.Mvvm;

public sealed class MacroDefinition
{
    public string Id { get; }
    public string Title { get; }
    public string? Category { get; }
    public string? Description { get; }
    public ICommand Command { get; }
    public object? CommandParameter { get; }

    public MacroDefinition(
        string id,
        string title,
        ICommand command,
        object? commandParameter = null,
        string? category = null,
        string? description = null)
    {
        Id = id;
        Title = title;
        Command = command;
        CommandParameter = commandParameter;
        Category = category;
        Description = description;
    }
}
