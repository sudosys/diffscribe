using DiffScribe.Commands.Models;

namespace DiffScribe.Commands;

public class GenerateCommand : ICommand
{
    public string Name => "generate";

    public string Description => "Generate a commit title based on the staged changes and desired commit structure.";

    public CommandArgument[] DefinedArguments => 
        [
            new("--auto-commit", "Commit automatically after generation", typeof(bool), optional: true),
            new("--auto-push", "Commit & push automatically after generation", typeof(bool), optional: true)
        ];
    
    public void Execute(Dictionary<string, object?> args)
    {
        Console.WriteLine(Description);
    }
}