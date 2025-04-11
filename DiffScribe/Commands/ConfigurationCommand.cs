using DiffScribe.Commands.Models;
using DiffScribe.Configuration;

namespace DiffScribe.Commands;

public class ConfigurationCommand : ICommand
{
    public string Name => "config";
    
    public string Description => "Display & edit tool configuration.";

    public CommandArgument[] DefinedArguments => 
        [
            new("--commit-structure", "Set the commit structure to shape the generated title.", typeof(string), optional: true),
            new("--api-key", "Set the OpenAI API key.", typeof(string), optional: true),
            new("--llm", "Select the OpenAI model to be used for generation.", typeof(LlmModel), optional: true),
            new("--auto-commit", "Commit automatically after generation.", typeof(bool), optional: true),
            new("--auto-push", "Commit & push automatically after generation.", typeof(bool), optional: true)
        ];
    
    public void Execute(Dictionary<string, string?> args)
    {
        Console.WriteLine(Description);
    }
}