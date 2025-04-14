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
                {
                    var selectedIdx = MakeCommitStyleSelection();
                    toolConfig.CommitStyle = ((CommitStyle)selectedIdx).ToString();
                    break;
                }
                case ApiKeyArg:
                    UpdateApiKey(value!.ToString()!);
                    break;
                case AutoCommitArg when value is not null:
                    toolConfig.AutoCommit = (bool)value;
                    break;
                case LlmArg:
                {
                    var selectedIdx = MakeModelSelection();
                    toolConfig.Llm = ((LlmModel)selectedIdx).ToApiName();
                    break;
                }
            }
        }
        
        _configHandler.UpdateConfiguration();
    }

    private void ShowCurrentConfiguration()
    {
        _configHandler.TryCreateConfigFile();
        _configHandler.ReadConfigFile();
    }

    private int MakeCommitStyleSelection()
    {
        var styles = EnumExtensions.CreateSelectionList<CommitStyle>();
        
        var selectedIdx = ConsoleWrapper.ShowSelectionList(styles, title: "Select a commit style that fits your needs, then press ENTER.");
        
        return selectedIdx;
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

    private int MakeModelSelection()
    {
        var models = EnumExtensions.CreateSelectionList<LlmModel>();
        
        var selectedIdx = ConsoleWrapper.ShowSelectionList(models, "Select a model for generation then press ENTER");
        return selectedIdx;
    }
}