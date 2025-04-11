namespace DiffScribe.Parser;

public struct CommandInfo(string name, Dictionary<string, string?> arguments)
{
    public string Name { get; set; } = name;
    public Dictionary<string, string?> Arguments { get; set; } = arguments;
}