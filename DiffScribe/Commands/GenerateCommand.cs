namespace DiffScribe.Commands;

public class GenerateCommand : ICommand
{
    public string Name => "generate";

    public string Description => "Generate a commit title based on the staged changes and desired commit structure.";

    public string[] DefinedArguments => ["—auto-commit", "—auto-push"];
    
    public void Execute(string[] args)
    {
        Console.WriteLine(Description);
    }
}