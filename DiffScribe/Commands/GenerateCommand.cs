using DiffScribe.AI;
using DiffScribe.Commands.Models;
using DiffScribe.Configuration;
using DiffScribe.Git;
using Microsoft.Extensions.DependencyInjection;
using TextCopy;

namespace DiffScribe.Commands;

public class GenerateCommand(IServiceProvider provider) : ICommand
{
    public string Name => "generate";

    public string Description => "Generate a commit title based on the staged changes and desired commit structure.";

    private const string AutoCommitArg = "--auto-commit";
    
    public CommandArgument[] DefinedArguments => 
        [
            new(AutoCommitArg, "Commit automatically after generation", typeof(bool), optional: true),
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
        var source = new CancellationTokenSource();
        
        ConsoleWrapper.ShowLoadingText("Generating commit message based on your changes", source.Token);
        
        var stagedDiffs = _gitRunner.GetStagedDiffs();
        
        var commitMessage =  _commitGenerator.GenerateCommitMessage(stagedDiffs);

        source.Cancel();
        
        return commitMessage;
    }

    private void PrintPostGeneration(string commitMessage)
    {
        Console.WriteLine();
        ConsoleWrapper.Success($"Generated commit message: {commitMessage}");
    }

    private void CopyToClipboard(string commitMessage)
    {
        ClipboardService.SetText(commitMessage);
        Console.WriteLine();
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
            
        if (proceed)
        {
            var success = _gitRunner.Commit(commitMessage);
            if (success)
            {
                ConsoleWrapper.Success("Changes are committed.");
            }
        }
        
        return proceed;
    }
}