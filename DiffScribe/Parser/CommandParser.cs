namespace DiffScribe.Parser;

public class CommandParser
{
    private readonly CommandInfo _rootCommandInfo = new(string.Empty, []);
   
    public CommandInfo Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return _rootCommandInfo;
        }
        
        var command = args[0];
        var arguments = args[1..];
        var parsedArguments = ParseArguments(arguments);

        return new CommandInfo(command, parsedArguments);
    }

    private Dictionary<string, object?> ParseArguments(string[] args)
    {
        Dictionary<string, object?> parsedArgs = new();
        
        var argsLength = args.Length;
        for (var i = 0; i < argsLength; i++)
        {
            if (!args[i].StartsWith("--"))
            {
                continue;
            }
            
            if (i + 1 < argsLength && !args[i + 1].StartsWith("--"))
            {
                parsedArgs.Add(args[i], ParseValue(args[i + 1]));
                i++;
            }
            else
            {
                parsedArgs.Add(args[i], null);
            }
        }

        return parsedArgs;
    }

    private object ParseValue(string value)
    {
        if (int.TryParse(value, out var parsedInt))
        {
            return parsedInt;
        }

        if (bool.TryParse(value, out var parsedBool))
        {
            return parsedBool;
        }
        
        return value;
    }
}