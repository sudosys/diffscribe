namespace DiffScribe.Parser;

public struct CommandInfo(string name, Dictionary<string, object?> arguments)
{
    public string Name { get; set; } = name;
    public Dictionary<string, object?> Arguments { get; set; } = arguments;
}