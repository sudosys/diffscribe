using DiffScribe.AI;
using DiffScribe.Commands.Models;
using DiffScribe.Configuration;
using DiffScribe.Configuration.Enums;
using DiffScribe.Extensions;
using Microsoft.Extensions.DependencyInjection;

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
    
    private readonly string[] _commitStyleSelections =
    [
        $"{CommitStyle.Minimal} (Short, one-line commit message.)",
        $"{CommitStyle.Standard} (Clear commit message with brief context.)",
        $"{CommitStyle.Detailed} (Descriptive commit message followed by an in-depth explanation.)"
    ];
    
    private readonly string[] _llmSelections =
    [
        $"{LlmModel.Gpt4o.ToDisplayName()} ({LlmModel.Gpt4o.GetStats()})",
        $"{LlmModel.Gpt4oMini.ToDisplayName()} ({LlmModel.Gpt4oMini.GetStats()})",
        $"{LlmModel.Gpt4_1.ToDisplayName()} ({LlmModel.Gpt4_1.GetStats()})",
        $"{LlmModel.Gpt4_1Mini.ToDisplayName()} ({LlmModel.Gpt4_1Mini.GetStats()})",
        $"{LlmModel.Gpt4_1Nano.ToDisplayName()} ({LlmModel.Gpt4_1Nano.GetStats()})",
    ];

    public void Execute(Dictionary<string, object?> args)
    {
        if (args.Count == 0)
        {
            ShowCurrentConfiguration();
            return;
        }
        
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

    private void MakeCommitStyleSelection(ref ToolConfiguration toolConfig)
    {
        _commitStyleSelections.UpdateSelectedOption<CommitStyle>(toolConfig.CommitStyle);

        var selectedIdx = ConsoleWrapper.ShowSelectionList(
            _commitStyleSelections,
            title: "Select a commit style that fits your needs:");

        if (selectedIdx == -1)
        {
            return;
        }

        var updatedStyle = ((CommitStyle)selectedIdx).ToString();
        toolConfig.CommitStyle = updatedStyle;
        
        ConsoleWrapper.Info($"Commit style has been updated to \"{updatedStyle}\".");
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
        _llmSelections.UpdateSelectedOption<LlmModel>(toolConfig.Llm);

        var selectedIdx = ConsoleWrapper.ShowSelectionList(
            _llmSelections,
            title: "Select a model for commit message generation:");

        if (selectedIdx == -1)
        {
            return;
        }

        var updatedModel = (LlmModel)selectedIdx;
        toolConfig.Llm = updatedModel.ToString();
        
        ConsoleWrapper.Info($"Model has been updated to \"{updatedModel.ToDisplayName()}\".");
    }
}