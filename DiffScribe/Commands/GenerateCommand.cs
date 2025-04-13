using DiffScribe.AI;
using DiffScribe.Commands.Models;
using DiffScribe.Git;
using Microsoft.Extensions.DependencyInjection;

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
    
    public void Execute(Dictionary<string, object?> args)
    {
        if (!ValidateVersionControl())
        {
            return;
        }

        ConsoleWrapper.ShowLoadingText("Generating commit message based on your changes");
        
        var stagedDiffs = _gitRunner.GetStagedDiffs();
        
        var commitMessage = _commitGenerator.GenerateCommitMessage(stagedDiffs);

        Console.WriteLine();
        ConsoleWrapper.Success($"Generated commit message: {commitMessage}");
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
}