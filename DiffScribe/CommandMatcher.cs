using DiffScribe.Commands;

namespace DiffScribe;

public class CommandMatcher(IServiceProvider provider)
{
    private readonly Dictionary<string, ICommand> _commandMappings = new()
    {
        { "root", new RootCommand() },
        { "generate", new GenerateCommand(provider) },
        { "g", new GenerateCommand(provider) },
        { "config", new ConfigurationCommand(provider) },
        { "c", new ConfigurationCommand(provider) },
        { "reset", new ResetCommand(provider) },
    };

    public bool TryMatch(string commandName, out ICommand? command)
    {
        if (_commandMappings.TryGetValue(commandName, out var cmd) 
            && DoesNameMatch(commandName, cmd.Name))
        {
            command = cmd;
            return true;
        }
        
        command = null;
        return false;
    }
    
    private static bool DoesNameMatch(string input, string commandName) 
        => string.Equals(input, commandName) || commandName.StartsWith(input);
}