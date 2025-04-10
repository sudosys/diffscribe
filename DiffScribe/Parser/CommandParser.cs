namespace DiffScribe.Parser;

public class CommandParser
{
    public static CommandInfo Parse(string[] args)
    {
        var command = args[0];
        var arguments = args[1..];
        return new CommandInfo(command, arguments);
    }
}