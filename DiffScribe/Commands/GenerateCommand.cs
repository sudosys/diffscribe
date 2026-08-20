using DiffScribe.AI;
using DiffScribe.AI.Models;
using DiffScribe.Commands.Models;
using DiffScribe.Configuration;
using DiffScribe.Configuration.Enums;
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
    private const string AmendArg = "--amend";
    private const string CommitStyleArg = "--commit-style";
    private const string CommitLengthArg = "--commit-len";
    private const string SteerArg = "--steer";

    private const int MaxSteerLength = 500;
    
    public CommandArgument[] DefinedArguments => 
        [
            new(AutoCommitArg, "Commit automatically after generation.", typeof(bool), optional: true),
            new(AmendArg, "Regenerate the message of the latest commit including the extra staged changes and amend it.", typeof(bool), optional: true),
            new(CommitStyleArg, "Select the commit style to be used for this generation only.", typeof(void), optional: true),
            new(CommitLengthArg, $"Maximum subject line length for this generation only. A preset ({CommitLengthResolver.GetPresetNames()}) or a character count between {CommitLengthResolver.MinCustomLength} and {CommitLengthResolver.MaxCustomLength}.", typeof(string), optional: true),
            new(SteerArg, "Steer the generation with an instruction in natural language.", typeof(string), optional: true),
        ];
    
    private readonly GitRunner _gitRunner = provider.GetRequiredService<GitRunner>();
    private readonly CommitGenerator _commitGenerator = provider.GetRequiredService<CommitGenerator>();
    private readonly ConfigHandler _configHandler = provider.GetRequiredService<ConfigHandler>();
    
    public void Execute(Dictionary<string, object?> args)
    {
        var amendRequested = GetFlagValue(args, AmendArg) ?? false;
        var autoCommitRequested = GetFlagValue(args, AutoCommitArg);

        if (amendRequested && autoCommitRequested == true)
        {
            ConsoleWrapper.Error($"\"{AmendArg}\" cannot be combined with \"{AutoCommitArg}\". The amended commit is committed by \"{AmendArg}\" itself.");
            return;
        }
        
        if (!TryGetMaxSubjectLengthOverride(args, out var maxSubjectLength)
            || !TryGetSteerInstruction(args, out var steer))
        {
            return;
        }
        
        if (!ValidateVersionControl(amendRequested))
        {
            return;
        }

        if (!_configHandler.IsApiKeySet())
        {
            ConsoleWrapper.Error("API key must be set in order to generate a commit message.");
            return;
        }

        // Selected last so that the menu is only shown once everything else is in place.
        var generationOptions = new GenerationOptions
        {
            CommitStyle = SelectCommitStyle(args),
            MaxSubjectLength = maxSubjectLength,
            Steer = steer
        };

        var previousCommitMessage = amendRequested ? _gitRunner.GetLastCommitMessage() : null;

        var commitMessage = GenerateCommitMessage(amendRequested, generationOptions);

        if (string.IsNullOrWhiteSpace(commitMessage))
        {
            ConsoleWrapper.Error("An empty commit message has been generated. Please try again.");
            return;
        }
        
        PrintPostGeneration(commitMessage, previousCommitMessage);

        var committed = amendRequested
            ? HandleAmend(commitMessage)
            : HandleAutoCommit(commitMessage, autoCommitRequested ?? _configHandler.Configuration.AutoCommit);

        if (!committed)
        {
            CopyToClipboard(commitMessage);
        }
    }

    /// <summary>
    /// Presence of a flag means it is enabled unless it is given an explicit boolean value.
    /// A null result means the flag is not given at all.
    /// </summary>
    private static bool? GetFlagValue(Dictionary<string, object?> args, string argumentName)
    {
        if (!args.TryGetValue(argumentName, out var value))
        {
            return null;
        }

        return value is bool givenValue ? givenValue : true;
    }

    private bool ValidateVersionControl(bool amendRequested)
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

        return amendRequested ? ValidateAmendable() : ValidateStagedFilesPresent();
    }

    private bool ValidateStagedFilesPresent()
    {
        if (!_gitRunner.StagedFilesPresent())
        {
            ConsoleWrapper.Error("Generation could not be started. No staged files found.");
            return false;
        }

        return true;
    }

    private bool ValidateAmendable()
    {
        if (!_gitRunner.CommitsPresent())
        {
            ConsoleWrapper.Error("There is no commit to amend in this repository.");
            return false;
        }

        if (!_gitRunner.StagedFilesPresent())
        {
            ConsoleWrapper.Info("No staged files found. The message is regenerated from the changes of the latest commit only.");
        }

        return true;
    }

    /// <summary>
    /// Prompts for a one-time commit style. Returns null when the argument is not given, 
    /// which leaves the configured commit style in charge.
    /// </summary>
    private CommitStyle? SelectCommitStyle(Dictionary<string, object?> args)
    {
        if (!args.ContainsKey(CommitStyleArg))
        {
            return null;
        }

        return CommitStyleSelector.Select(
            _configHandler.Configuration.CommitStyle,
            "Select the commit style to be used for this generation:");
    }

    private bool TryGetMaxSubjectLengthOverride(Dictionary<string, object?> args, out int? maxSubjectLength)
    {
        maxSubjectLength = null;
        
        if (!args.TryGetValue(CommitLengthArg, out var value))
        {
            return true;
        }

        if (!CommitLengthResolver.TryResolve(value?.ToString(), out var resolvedLength, out var error))
        {
            ConsoleWrapper.Error(error!);
            return false;
        }

        maxSubjectLength = resolvedLength;
        return true;
    }

    private bool TryGetSteerInstruction(Dictionary<string, object?> args, out string? steer)
    {
        steer = null;
        
        if (!args.TryGetValue(SteerArg, out var value))
        {
            return true;
        }

        var instruction = value?.ToString()?.Trim();

        if (string.IsNullOrEmpty(instruction))
        {
            ConsoleWrapper.Error($"\"{SteerArg}\" requires an instruction. "
                                 + $"e.g. {AppInfo.ExecutableName} generate {SteerArg} \"mention the ticket id in the scope\"");
            return false;
        }

        if (instruction.Length > MaxSteerLength)
        {
            ConsoleWrapper.Error($"The instruction given to \"{SteerArg}\" must not be longer than {MaxSteerLength} characters.");
            return false;
        }

        steer = instruction;
        return true;
    }

    private string GenerateCommitMessage(bool amendRequested, GenerationOptions? generationOptions)
    {
        var commitMessage = string.Empty;
        ConsoleWrapper.TakeActionWithLoadingText(() =>
        {
            var diffs = amendRequested ? _gitRunner.GetAmendDiffs() : _gitRunner.GetStagedDiffs();
        
            commitMessage =  _commitGenerator.GenerateCommitMessage(diffs, generationOptions);
        },
        amendRequested
            ? "Regenerating the commit message of the latest commit"
            : "Generating commit message based on your changes");
        
        return commitMessage;
    }

    private void PrintPostGeneration(string commitMessage, string? previousCommitMessage)
    {
        Console.WriteLine();

        var table = new Table();
        if (string.IsNullOrEmpty(previousCommitMessage))
        {
            table
                .AddColumn("Generated commit message")
                .AddRow(Markup.Escape(commitMessage));
        }
        else
        {
            table
                .AddColumns("Latest commit message", "Regenerated commit message")
                .AddRow(Markup.Escape(previousCommitMessage), Markup.Escape(commitMessage));
        }
        
        AnsiConsole.Write(table);
        Console.WriteLine();
    }

    private void CopyToClipboard(string commitMessage)
    {
        ClipboardService.SetText(commitMessage);
        ConsoleWrapper.Info("Commit message is copied to clipboard!");
    }

    private bool HandleAutoCommit(string commitMessage, bool autoCommit)
    {
        if (!autoCommit)
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
        if (!committed)
        {
            ConsoleWrapper.Error("Changes could not be committed.");
            return false;
        }
        
        ConsoleWrapper.Success("Changes are committed.");

        return true;
    }

    private bool HandleAmend(string commitMessage)
    {
        var proceed = ConsoleWrapper
            .ShowConfirmation("Amend the latest commit with the regenerated message above?");

        if (!proceed)
        {
            ConsoleWrapper.Info("Amend operation aborted.");
            return false;
        }

        var amended = _gitRunner.Amend(commitMessage);
        if (!amended)
        {
            ConsoleWrapper.Error("The latest commit could not be amended.");
            return false;
        }
        
        ConsoleWrapper.Success("The latest commit has been amended with the regenerated message.");

        return true;
    }
}
