using DiffScribe.Commands;
using DiffScribe.Commands.Models;
using DiffScribe.Suggestions;

namespace DiffScribe;

public class ArgumentValidator
{
    public bool Validate(ICommand command, Dictionary<string, object?> givenArguments)
    {
        var definedArguments = command.DefinedArguments;
        
        foreach (var (argument, value) in givenArguments)
        {
            var argumentDefinition = definedArguments.SingleOrDefault(arg => arg.Name == argument);

            if (argumentDefinition == null)
            {
                ReportUnknownArgument(command, argument);
                return false;
            }

            if (!ValidateValueType(value, argumentDefinition))
            {
                ReportInvalidValue(command, argument, value, argumentDefinition);
                return false;
            }
        }
        
        return true;
    }

    private void ReportUnknownArgument(ICommand command, string argument)
    {
        ConsoleWrapper.Error($"Unknown argument: {argument}");

        var recommendation = SuggestionEngine
            .BuildRecommendation(argument, command.DefinedArguments.Select(arg => arg.Name));

        ConsoleWrapper.Info(string.IsNullOrEmpty(recommendation)
            ? $"Run \"{AppInfo.ExecutableName} help --{command.Name}\" to see the arguments of this command."
            : recommendation);
    }

    private void ReportInvalidValue(
        ICommand command, 
        string argument, 
        object? value, 
        CommandArgument argumentDefinition)
    {
        if (argumentDefinition.Type == typeof(void))
        {
            ConsoleWrapper.Error($"The argument {argument} does not take a value.");
            ConsoleWrapper.Info($"Run \"{AppInfo.ExecutableName} {command.Name} {argument}\" to pick a value interactively.");
            return;
        }

        if (value is null)
        {
            ConsoleWrapper.Error($"The argument {argument} requires a value of type: {argumentDefinition.Type.Name}");
            return;
        }
        
        ConsoleWrapper.Error($"Invalid value type for the argument: {argument}. Expected: {argumentDefinition.Type.Name}");
    }

    private bool ValidateValueType(object? value, CommandArgument argumentDefinition) =>
        value switch
        {
            // A numeric value is also accepted where text is expected, 
            // since values such as an API key or a custom commit length are parsed as integers.
            int => argumentDefinition.Type == typeof(int) || argumentDefinition.Type == typeof(string),
            bool => argumentDefinition.Type == typeof(bool),
            null => argumentDefinition.Type == typeof(bool) || argumentDefinition.Type == typeof(void),
            _ => argumentDefinition.Type == typeof(string)
        };
}
