using DiffScribe.Parser;

namespace DiffScribe;

public class CommandDispatcher(ArgumentValidator argumentValidator, CommandMatcher commandMatcher)
{
    public void Dispatch(CommandInfo commandInfo)
    {
        if (!commandMatcher.TryMatch(commandInfo.Name, out var command) || command == null)
        {
            ConsoleWrapper.Error($"Unknown command: {commandInfo.Name}");
            return;
        }

        if (!argumentValidator.Validate(command.DefinedArguments, commandInfo.Arguments))
        {
            return;
        }
        
        try
        {
            command.Execute(commandInfo.Arguments);
        }
        catch (Exception e)
        {
            Console.WriteLine();
            ConsoleWrapper.Error($"{e.GetType()}: {e.Message}");
        }
    }
}