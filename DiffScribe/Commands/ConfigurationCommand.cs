using System.Collections.Immutable;
using DiffScribe.Commands.Models;
using DiffScribe.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiffScribe.Commands;

public class ConfigurationCommand(IServiceProvider provider) : ICommand
{
    public string Name => "config";
    
    public string Description => "Display & edit tool configuration.";
    
    private const string CommitStructureArg = "--commit-structure";
    private const string ApiKeyArg = "--api-key";
    private const string LlmArg = "--llm";
    private const string AutoCommitArg = "--auto-commit";
    private const string AutoPushArg = "--auto-push";

    public CommandArgument[] DefinedArguments => 
        [
            new(CommitStructureArg, "Set the commit structure to shape the generated title.", typeof(string), optional: true),
            new(ApiKeyArg, "Set the OpenAI API key.", typeof(string), optional: true),
            new(LlmArg, "Select the OpenAI model to be used for generation.", typeof(void), optional: true),
            new(AutoCommitArg, "Commit automatically after generation.", typeof(bool), optional: true),
            new(AutoPushArg, "Commit & push automatically after generation.", typeof(bool), optional: true)
        ];
    
    public void Execute(Dictionary<string, object?> args)
    {
        var configHandler = provider.GetRequiredService<ConfigHandler>();

        if (args.Count == 0)
        {
            ShowCurrentConfiguration(configHandler);
            return;
        }
        
        var toolConfig = configHandler.GetConfigurationFromFile();

        foreach (var (arg, value) in args)
        {
            switch (arg)
            {
                case CommitStructureArg when value is not null:
                    toolConfig.CommitStructure = value.ToString()!;
                    break;
                case ApiKeyArg when value is not null:
                    toolConfig.ApiKey = value.ToString()!;
                    break;
                case AutoCommitArg when value is not null:
                    toolConfig.AutoCommit = (bool)value;
                    break;
                case AutoPushArg when value is not null:
                    toolConfig.AutoPush = (bool)value;
                    break;
                case LlmArg:
                    var selectedIdx = MakeModelSelection();
                    toolConfig.Llm = ((LlmModel)selectedIdx).ToApiName();
                    break;
            }
        }
        
        configHandler.UpdateConfiguration(toolConfig);
    }

    private void ShowCurrentConfiguration(ConfigHandler configHandler)
    {
        configHandler.TryCreateConfigFile();
        configHandler.ReadConfigFile();
    }

    private int MakeModelSelection()
    {
        var models = Enum.GetValues<LlmModel>()
            .Select(v => v.ToApiName())
            .ToImmutableArray();
        
        var selectedIdx = ConsoleWrapper.ShowSelectionList(models, "Select a model for generation then press ENTER");
        return selectedIdx;
    }
}