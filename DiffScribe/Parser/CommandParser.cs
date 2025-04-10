namespace DiffScribe.Parser;

public class CommandParser
{
    private readonly CommandInfo _rootCommandInfo = new("root", []);
   
    public CommandInfo Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return _rootCommandInfo;
        }
        
        var command = args[0];
        var arguments = args[1..];
        return new CommandInfo(command, arguments);
    }
}