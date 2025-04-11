using DiffScribe.Commands.Models;

namespace DiffScribe;

public class ArgumentValidator
{
    public bool Validate(CommandArgument[] definedArguments, Dictionary<string, string?> givenArguments)
    {
        foreach (var (argument, value) in givenArguments)
        {
            var argumentDefinition = definedArguments.SingleOrDefault(arg => arg.Name == argument);

            if (argumentDefinition == null)
            {
                ConsoleWrapper.Error($"Unknown argument: {argument}");
                return false;
            }

            if (!ValidateValueType(value, argumentDefinition))
            {
                ConsoleWrapper.Error($"Invalid value type for the argument: {argument}. Expected: {argumentDefinition.Type.Name}");
                return false;
            }
        }
        
        return true;
    }

    private bool ValidateValueType(object? value, CommandArgument argumentDefinition) =>
        value switch
        {
            int => argumentDefinition.Type == typeof(int),
            bool => argumentDefinition.Type == typeof(bool),
            null => argumentDefinition.Type == typeof(bool) || argumentDefinition.Type == typeof(void),
            _ => argumentDefinition.Type == typeof(string)
        };
}