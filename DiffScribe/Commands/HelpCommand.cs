using DiffScribe.Commands.Models;
using Spectre.Console;

namespace DiffScribe.Commands;

public class HelpCommand : ICommand
{
    public string Name => "help";

    public string Description => "Returns the intention of the command and its arguments in a table format.";
    
    public CommandArgument[] DefinedArguments { get; }
    
    private readonly CommandMatcher _commandMatcher;

    public HelpCommand(CommandMatcher commandMatcher)
    {
        _commandMatcher = commandMatcher;
        
        DefinedArguments = _commandMatcher
            .GetDefinedCommandNames()
            .Select(cmd => new CommandArgument($"--{cmd}", string.Empty, typeof(void), optional: true))
            .ToArray();
    }
    
    public void Execute(Dictionary<string, object?> args)
    {
        switch (args.Count)
        {
            case 0:
                PrintCommandList();
                break;
            case 1:
                PrintCommandHelp(args.First().Key);
                break;
            case > 1:
                ConsoleWrapper.Error("Provide a single command name to get help.");
                break;
        }
    }

    private void PrintCommandList()
    {
        AnsiConsole.Write(CreateCommandsTable());
        ConsoleWrapper.Info("To get help for a specific command: \"help --<command>\"");
    }

    private Table CreateCommandsTable()
    {
        var table = new Table().AddColumns("Command", "Description");
        foreach (var arg in DefinedArguments)
        {
            _commandMatcher.TryMatch(ParseCmdName(arg.Name), out var cmd);

            if (cmd is not null)
            {
                table.AddRow(cmd.Name, cmd.Description);
            }
        }

        return table;
    }

    private void PrintCommandHelp(string arg)
    {
        var cmdName = ParseCmdName(arg);
        if (!_commandMatcher.TryMatch(cmdName, out var cmd) || cmd is null)
        {
            ConsoleWrapper.Error($"No such command exists: \"{cmdName}\"");
            return;
        }
        
        Console.WriteLine($"{cmd.Name}: {cmd.Description}\n");

        if (cmd.DefinedArguments.Length > 0)
        {
            AnsiConsole.Write(CreateCommandTable(cmd));
        }
        else
        {
            ConsoleWrapper.Info("This command doesn't have any arguments.");
        }
    }
    
    private string ParseCmdName(string argument) => argument.TrimStart('-');

    private Table CreateCommandTable(ICommand command)
    {
        var table = new Table().AddColumns("Argument", "Description", "Type", "Optional");

        foreach (var commandArg in command.DefinedArguments)
        {
            table.AddRow(
                commandArg.Name,
                commandArg.Description,
                commandArg.Type.Name,
                commandArg.Optional.ToString());
        }
        
        return table;
    }
}