namespace DiffScribe.Parser;

public class CommandParser
{
    private const string ArgumentPrefix = "--";
    
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
            if (!IsArgumentName(args[i]))
            {
                continue;
            }

            var argumentName = args[i];
            List<string> valueTokens = [];
            
            // Every token until the next argument belongs to the current one, 
            // which keeps unquoted values such as "--steer keep it short" intact.
            while (i + 1 < argsLength && !IsArgumentName(args[i + 1]))
            {
                valueTokens.Add(args[i + 1]);
                i++;
            }

            parsedArgs[argumentName] = valueTokens.Count switch
            {
                0 => null,
                1 => ParseValue(valueTokens[0]),
                _ => string.Join(' ', valueTokens)
            };
        }

        return parsedArgs;
    }

    private bool IsArgumentName(string token) => token.StartsWith(ArgumentPrefix);

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
