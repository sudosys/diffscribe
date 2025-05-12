using DiffScribe.Commands.Models;

namespace DiffScribe.Commands;

public class VersionCommand : ICommand
{
    public string Name => "version";

    public string Description => "Returns the current version of the CLI tool.";

    public CommandArgument[] DefinedArguments => [];
    
    public void Execute(Dictionary<string, object?> args)
    {
        Console.WriteLine($"v{AppInfo.Version}");
    }
}