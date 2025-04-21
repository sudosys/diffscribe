using DiffScribe.Parser;

namespace DiffScribe;

public class CommandDispatcher(ArgumentValidator argumentValidator, CommandMatcher commandMatcher)
{
    public void Dispatch(CommandInfo commandInfo)
    {
        if (commandMatcher.TryMatch(commandInfo.Name, out var command)
            && argumentValidator.Validate(command!.DefinedArguments, commandInfo.Arguments))
        {
            command.Execute(commandInfo.Arguments);
        }
        else
        {
            ConsoleWrapper.Error($"Unknown command: {commandInfo.Name}");
        }
    }
}