using DiffScribe.Commands;
using DiffScribe.Parser;

namespace DiffScribe;

public class CommandDispatcher(ArgumentValidator argumentValidator)
{
    public void Dispatch(CommandInfo commandInfo, ICommand command)
    {
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