using DiffScribe.Commands;
using DiffScribe.Parser;

namespace DiffScribe;

public class CommandDispatcher
{
    private readonly Dictionary<string, ICommand> _commandMappings = new()
    {
        { "generate", new GenerateCommand() }
    };
    
    public void Dispatch(CommandInfo commandInfo)
    {
        if (_commandMappings.TryGetValue(commandInfo.Name, out var command) 
            && commandInfo.Name == command.Name)
        {
            command.Execute(commandInfo.Arguments);
        }
        else
        {
            Console.WriteLine($"Unknown command: {commandInfo.Name}");
        }
    }
}