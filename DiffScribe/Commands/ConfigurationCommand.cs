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
    
    public string Description => "Display & edit tool configuration.";
    
    private const string CommitStyleArg = "--commit-style";
    private const string ApiKeyArg = "--api-key";
    private const string LlmArg = "--llm";
    private const string AutoCommitArg = "--auto-commit";

    public CommandArgument[] DefinedArguments => 
        [
            new(CommitStyleArg, "Set the commit style to specify the verbosity of the commit message.", typeof(void), optional: true),
            new(ApiKeyArg, "Set the OpenAI API key.", typeof(string), optional: true),
            new(LlmArg, "Select the OpenAI model to be used for generation.", typeof(void), optional: true),
            new(AutoCommitArg, "Commit automatically after generation.", typeof(bool), optional: true),
        ];
    
    private readonly ConfigHandler _configHandler = provider.GetRequiredService<ConfigHandler>();
    private readonly OpenAiClient _openAiClient = provider.GetRequiredService<OpenAiClient>();
    
    private Dictionary<CommitStyle, string> _commitStyleSelections = new()
    {
        { CommitStyle.Minimal, $"{CommitStyle.Minimal} (Short, one-line commit message.)" },
        { CommitStyle.Standard, $"{CommitStyle.Standard} (Clear commit message with brief context.)" },
        { CommitStyle.Detailed, $"{CommitStyle.Detailed} (Descriptive commit message followed by an in-depth explanation.)" },
    };

    private Dictionary<LlmModel, string> _llmSelections = new()
    {
        { LlmModel.Gpt4o, $"{LlmModel.Gpt4o.ToDisplayName()} ({LlmModel.Gpt4o.GetStats()})" },
        { LlmModel.Gpt4oMini, $"{LlmModel.Gpt4oMini.ToDisplayName()} ({LlmModel.Gpt4oMini.GetStats()})" },
        { LlmModel.Gpt4_1, $"{LlmModel.Gpt4_1.ToDisplayName()} ({LlmModel.Gpt4_1.GetStats()})" },
        { LlmModel.Gpt4_1Mini, $"{LlmModel.Gpt4_1Mini.ToDisplayName()} ({LlmModel.Gpt4_1Mini.GetStats()})" },
        { LlmModel.Gpt4_1Nano, $"{LlmModel.Gpt4_1Nano.ToDisplayName()} ({LlmModel.Gpt4_1Nano.GetStats()})" }
    };

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
                    MakeCommitStyleSelection(ref toolConfig);
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

    private void MakeCommitStyleSelection(ref ToolConfiguration toolConfig)
    { 
       EnumExtensions.UpdateSelectedOption(ref _commitStyleSelections, toolConfig.CommitStyle);

       var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select a commit style that fits your needs:")
            .AddChoices(_commitStyleSelections.Values));
       
       var selectedCommitStyle = _commitStyleSelections
           .First(p => p.Value == selection)
           .Key
           .ToString();
       
       toolConfig.CommitStyle = selectedCommitStyle;
       ConsoleWrapper.Info($"Commit style has been updated to \"{selectedCommitStyle}\".");
    }

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