using DiffScribe.Commands.Models;

namespace DiffScribe.Commands;

public class RootCommand : ICommand
{
    public string Name => "root";

    public string Description => "Root command of the CLI tool.";

    public CommandArgument[] DefinedArguments => [];
    
    public void Execute(Dictionary<string, object?> args)
    {
        Console.WriteLine(Description);
    }
}