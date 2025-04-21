using DiffScribe.Commands.Models;

namespace DiffScribe.Commands;

public class RootCommand(CommandMatcher commandMatcher) : ICommand
{
    public string Name => "root";

    public string Description => "Root command of the CLI tool.";

    public CommandArgument[] DefinedArguments => [];
    
    public void Execute(Dictionary<string, object?> _)
    {
        PrintToolHeader();
        RunHelpCommand();
    }

    private void PrintToolHeader() =>
        Console.WriteLine("""
                          >>========================================================================================<<
                          || ██████████    ███    ██████ ██████ █████████                      ████████             ||
                          ||░░███░░░░███  ░░░    ███░░█████░░█████░░░░░███                    ░░░░░███              ||
                          || ░███   ░░███ ████  ░███ ░░░███ ░░░███    ░░░   ██████  ████████  ████░███████   ██████ ||
                          || ░███    ░███░░███ ██████████████ ░░█████████  ███░░███░░███░░███░░███░███░░███ ███░░███||
                          || ░███    ░███ ░███░░░███░░░░███░   ░░░░░░░░███░███ ░░░  ░███ ░░░  ░███░███ ░███░███████ ||
                          || ░███    ███  ░███  ░███   ░███    ███    ░███░███  ███ ░███      ░███░███ ░███░███░░░  ||
                          || ██████████   █████ █████  █████  ░░█████████ ░░██████  █████     ████████████ ░░██████ ||
                          ||░░░░░░░░░░   ░░░░░ ░░░░░  ░░░░░    ░░░░░░░░░   ░░░░░░  ░░░░░     ░░░░░░░░░░░░   ░░░░░░  ||
                          >>========================================================================================<<
                          """);

    private void RunHelpCommand()
    {
        var helpCmd = new HelpCommand(commandMatcher);
        helpCmd.Execute(new Dictionary<string, object?>());
    }
}