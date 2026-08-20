using DiffScribe.AI;
using DiffScribe.Commands.Models;
using DiffScribe.Configuration;
using DiffScribe.Configuration.Enums;
using DiffScribe.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace DiffScribe.Commands;

public class ConfigurationCommand(IServiceProvider provider) : ICommand
{
    public string Name => "config";
    
    public string Description => "Displays or edits tool configuration.";
    
    private const string CommitStyleArg = "--commit-style";
    private const string CommitLengthArg = "--commit-len";
    private const string ApiKeyArg = "--api-key";
    private const string LlmArg = "--llm";
    private const string AutoCommitArg = "--auto-commit";

    private const string CustomCommitLengthOption = "Custom (Set your own subject line length.)";

    public CommandArgument[] DefinedArguments => 
        [
            new(CommitStyleArg, "Set the commit style to specify the verbosity of the commit message.", typeof(void), optional: true),
            new(CommitLengthArg, "Set the maximum length of the commit message subject line.", typeof(void), optional: true),
            new(ApiKeyArg, "Set the OpenAI API key.", typeof(string), optional: true),
            new(LlmArg, "Select the OpenAI model to be used for generation.", typeof(void), optional: true),
            new(AutoCommitArg, "Commit automatically after generation.", typeof(bool), optional: true),
        ];
    
    private readonly ConfigHandler _configHandler = provider.GetRequiredService<ConfigHandler>();
    private readonly OpenAiClient _openAiClient = provider.GetRequiredService<OpenAiClient>();
    
    private readonly Dictionary<CommitLength, string> _commitLengthSelections = new()
    {
        { CommitLength.Short, FormatCommitLengthSelection(CommitLength.Short) },
        { CommitLength.Standard, FormatCommitLengthSelection(CommitLength.Standard) },
        { CommitLength.Long, FormatCommitLengthSelection(CommitLength.Long) },
    };

    private Dictionary<LlmModel, string> _llmSelections = Enum
        .GetValues<LlmModel>()
        .ToDictionary(model => model, model => $"{model.ToDisplayName()} ({model.GetStats()})");

    public void Execute(Dictionary<string, object?> args)
    {
        if (args.Count == 0)
        {
            ShowCurrentConfiguration();
            return;
        }
        
        ValidateArgumentCombination(args);
        
        var toolConfig = _configHandler.Configuration;
        foreach (var (arg, value) in args)
        {
            switch (arg)
            {
                case CommitStyleArg:
                    MakeCommitStyleSelection(toolConfig);
                    break;
                case CommitLengthArg:
                    MakeCommitLengthSelection(toolConfig);
                    break;
                case ApiKeyArg:
                    UpdateApiKey(value!.ToString()!);
                    break;
                case AutoCommitArg when value is not null:
                    toolConfig.AutoCommit = (bool)value;
                    break;
                case LlmArg:
                    MakeModelSelection(ref toolConfig);
                    break;
            }
        }
        
        _configHandler.UpdateConfiguration();
    }

    private void ShowCurrentConfiguration()
    {
        _configHandler.TryCreateConfigFile();
        _configHandler.PrintCurrentConfigAsTable();
    }

    private void ValidateArgumentCombination(Dictionary<string, object?> args)
    {
        var interactiveArgs = DefinedArguments
            .Where(a => a.Type == typeof(void));

        var anyInteractiveArgGiven = interactiveArgs
            .Any(a => args.ContainsKey(a.Name));
        
        var interactiveArgCombined = anyInteractiveArgGiven && args.Count > 1;

        if (interactiveArgCombined)
        {
            throw new InvalidOperationException("Interactive arguments cannot be combined with other arguments. Execute them separately.");
        }
    }

    private void MakeCommitStyleSelection(ToolConfiguration toolConfig)
    { 
        var selectedCommitStyle = CommitStyleSelector.Select(
            toolConfig.CommitStyle, 
            "Select a commit style that fits your needs:");
       
        toolConfig.CommitStyle = selectedCommitStyle.ToString();
        ConsoleWrapper.Info($"Commit style has been updated to \"{selectedCommitStyle}\".");
    }

    private void MakeCommitLengthSelection(ToolConfiguration toolConfig)
    {
        var customLengthConfigured = int.TryParse(toolConfig.CommitLength, out var configuredCustomLength);
        
        var presetSelections = new Dictionary<CommitLength, string>(_commitLengthSelections);
        if (!customLengthConfigured)
        {
            EnumExtensions.UpdateSelectedOption(ref presetSelections, toolConfig.CommitLength);
        }

        var customSelection = customLengthConfigured
            ? Markup.Escape($"[X] {CustomCommitLengthOption} (Currently {configuredCustomLength} characters.)")
            : CustomCommitLengthOption;
        
        var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select the maximum length of the commit message subject line:")
            .AddChoices(presetSelections.Values.Append(customSelection)));

        var selectedCommitLength = selection == customSelection
            ? PromptForCustomCommitLength().ToString()
            : presetSelections.First(p => p.Value == selection).Key.ToString();

        toolConfig.CommitLength = selectedCommitLength;
        ConsoleWrapper.Info($"Commit length has been updated to \"{selectedCommitLength}\".");
    }

    private int PromptForCustomCommitLength() =>
        AnsiConsole.Prompt(new TextPrompt<int>(
                $"Enter the maximum subject line length in characters "
                + $"({CommitLengthResolver.MinCustomLength}-{CommitLengthResolver.MaxCustomLength}):")
            .Validate(length => CommitLengthResolver.IsValidCustomLength(length)
                ? ValidationResult.Success()
                : ValidationResult.Error($"[red]A commit length between {CommitLengthResolver.MinCustomLength} "
                                         + $"and {CommitLengthResolver.MaxCustomLength} characters is required.[/]")));

    private static string FormatCommitLengthSelection(CommitLength length)
        => $"{length} (Up to {length.ToMaxSubjectLength()} characters. {length.GetDescription()})";

    private void UpdateApiKey(string apiKey)
    {
        if (!_openAiClient.TestApiKeyValidity(apiKey))
        {
            ConsoleWrapper.Error("The given API key is invalid. API key configuration is not updated.");
            return;
        }

        _configHandler.UpdateApiKey(apiKey);
        
        ConsoleWrapper.Success("The API key has been updated.");
    }

    private void MakeModelSelection(ref ToolConfiguration toolConfig)
    {
        EnumExtensions.UpdateSelectedOption(ref _llmSelections, toolConfig.Llm);
        
        var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select a model for commit message generation:")
            .AddChoices(_llmSelections.Values));
       
        var selectedLlm = _llmSelections
            .First(p => p.Value == selection)
            .Key;

        toolConfig.Llm = selectedLlm.ToString();
        ConsoleWrapper.Info($"Model has been updated to \"{selectedLlm.ToDisplayName()}\".");
    }
}
