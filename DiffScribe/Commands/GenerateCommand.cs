using DiffScribe.AI;
using DiffScribe.Commands.Models;
using DiffScribe.Configuration;
using DiffScribe.Git;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using TextCopy;

namespace DiffScribe.Commands;

public class GenerateCommand(IServiceProvider provider) : ICommand
{
    public string Name => "generate";

    public string Description => "Generates a commit message based on the staged changes.";

    private const string AutoCommitArg = "--auto-commit";
    
    public CommandArgument[] DefinedArguments => 
        [
            new(AutoCommitArg, "Commit automatically after generation.", typeof(bool), optional: true),
        ];
    
    private readonly GitRunner _gitRunner = provider.GetRequiredService<GitRunner>();
    private readonly CommitGenerator _commitGenerator = provider.GetRequiredService<CommitGenerator>();
    private readonly ConfigHandler _configHandler = provider.GetRequiredService<ConfigHandler>();
    
    public void Execute(Dictionary<string, object?> args)
    {
        if (!ValidateVersionControl())
        {
            return;
        }

        if (!_configHandler.IsApiKeySet())
        {
            ConsoleWrapper.Error("API key must be set in order to generate a commit message.");
            return;
        }

        var commitMessage = GenerateCommitMessage();
        
        PrintPostGeneration(commitMessage);
        
        var autoCommitted = HandleAutoCommit(commitMessage, args);

        if (!autoCommitted)
        {
            CopyToClipboard(commitMessage);
        }
    }

    private bool ValidateVersionControl()
    {
        if (!_gitRunner.IsGitInstalled())
        {
            ConsoleWrapper.Error("git is required to run this command.");
            return false;
        }

        if (!_gitRunner.IsInsideGitRepository())
        {
            ConsoleWrapper.Error("Command must be run inside a git repository.");
            return false;
        }

        if (!_gitRunner.StagedFilesPresent())
        {
            ConsoleWrapper.Error("Generation could not be started. No staged files found.");
            return false;
        }

        return true;
    }

    private string GenerateCommitMessage()
    {
        var commitMessage = string.Empty;
        ConsoleWrapper.TakeActionWithLoadingText(() =>
        {
            var stagedDiffs = _gitRunner.GetStagedDiffs();
        
            commitMessage =  _commitGenerator.GenerateCommitMessage(stagedDiffs);
        },
        "Generating commit message based on your changes");
        
        return commitMessage;
    }

    private void PrintPostGeneration(string commitMessage)
    {
        Console.WriteLine();
        AnsiConsole.Write(new Table()
            .AddColumn("Generated commit message")
            .AddRow(commitMessage));
        Console.WriteLine();
    }

    private void CopyToClipboard(string commitMessage)
    {
        ClipboardService.SetText(commitMessage);
        ConsoleWrapper.Info("Commit message is copied to clipboard!");
    }

    private bool HandleAutoCommit(string commitMessage, Dictionary<string, object?> args)
    {
        if (!args.TryGetValue(AutoCommitArg, out _)
            && !_configHandler.Configuration.AutoCommit)
        {
            return false;
        }
        
        var proceed = 
            ConsoleWrapper.ShowConfirmation("Proceed to commit your changes with the message above?");

        if (!proceed)
        {
            return false;
        }
        
        var committed = _gitRunner.Commit(commitMessage);
        if (committed)
        {
            ConsoleWrapper.Success("Changes are committed.");
        }

        return true;
    }
}