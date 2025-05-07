using DiffScribe.Parser;
using DiffScribe.Update;

namespace DiffScribe;

public class CommandDispatcher(ArgumentValidator argumentValidator, CommandMatcher commandMatcher, AppUpdater appUpdater)
{
    public async Task Dispatch(CommandInfo commandInfo)
    {
        if (!commandMatcher.TryMatch(commandInfo.Name, out var command) || command == null)
        {
            return;
        }
        
        await appUpdater.CheckForUpdates(command.Name);

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
            ConsoleWrapper.Error($"{e.GetType()}: {e.Message}");
        }
    }
}