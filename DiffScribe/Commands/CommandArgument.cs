namespace DiffScribe.Commands;

public class CommandArgument(string name, string description, Type type, bool optional)
{
    public string Name { get; } = name;
    
    public string Description { get; } = description;
    
    public Type Type { get; } = type;
    
    public bool Optional { get; } = optional;
}