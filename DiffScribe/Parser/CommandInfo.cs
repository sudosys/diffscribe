namespace DiffScribe.Parser;

public struct CommandInfo(string name, string[] arguments)
{
    public string Name { get; set; } = name;
    public string[] Arguments { get; set; } = arguments;
}