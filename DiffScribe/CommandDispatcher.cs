using DiffScribe.Commands;
using DiffScribe.Parser;

namespace DiffScribe;

public class CommandDispatcher(ArgumentValidator argumentValidator)
{
    private readonly Dictionary<string, ICommand> _commandMappings = new()
    {
        { "root", new RootCommand() },
        { "generate", new GenerateCommand() },
        { "config", new ConfigurationCommand() }
    };
    
    public void Dispatch(CommandInfo commandInfo)
    {
        if (_commandMappings.TryGetValue(commandInfo.Name, out var command) 
            && commandInfo.Name == command.Name)
        {
            if (argumentValidator.Validate(command.DefinedArguments, commandInfo.Arguments))
            {
                command.Execute(commandInfo.Arguments);
            }
        }
        else
        {
            ConsoleWrapper.Error($"Unknown command: {commandInfo.Name}");
        }
    }
}