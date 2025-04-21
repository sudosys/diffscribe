using DiffScribe.Commands;

namespace DiffScribe;

public class CommandMatcher
{
    private readonly Dictionary<string, Lazy<ICommand>> _commandMappings;

    public CommandMatcher(IServiceProvider provider)
    {
        _commandMappings = new Dictionary<string, Lazy<ICommand>>
        {
            { "root", new Lazy<ICommand>(() => new RootCommand()) },
            { "generate", new Lazy<ICommand>(() => new GenerateCommand(provider)) },
            { "g", new Lazy<ICommand>(() => new GenerateCommand(provider)) },
            { "config", new Lazy<ICommand>(() => new ConfigurationCommand(provider)) },
            { "c", new Lazy<ICommand>(() => new ConfigurationCommand(provider)) },
            { "reset", new Lazy<ICommand>(() => new ResetCommand(provider)) },
            { "help", new Lazy<ICommand>(() => new HelpCommand(this)) }
        };
    }

    public bool TryMatch(string commandName, out ICommand? command)
    {
        if (_commandMappings.TryGetValue(commandName, out var cmd) 
            && DoesNameMatch(commandName, cmd.Value.Name))
        {
            command = cmd.Value;
            return true;
        }
        
        command = null;
        return false;
    }
    
    private static bool DoesNameMatch(string input, string commandName) 
        => string.Equals(input, commandName) || commandName.StartsWith(input);
    
    public string[] GetDefinedCommandNames() 
        => _commandMappings.Keys
            .Skip(1)
            .Where(key => key.Length > 1)
            .ToArray();
}