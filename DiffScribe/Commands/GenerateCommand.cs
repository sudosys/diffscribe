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

    public CommandArgument[] DefinedArguments => 
        [
            new("--auto-commit", "Commit automatically after generation", typeof(bool), optional: true),
            new("--auto-push", "Commit & push automatically after generation", typeof(bool), optional: true)
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
        
        CopyToClipboard(commitMessage);
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
        ConsoleWrapper.ShowLoadingText("Generating commit message based on your changes");
        
        var stagedDiffs = _gitRunner.GetStagedDiffs();
        
        return _commitGenerator.GenerateCommitMessage(stagedDiffs);
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
}