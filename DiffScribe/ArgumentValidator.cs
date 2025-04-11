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

    private bool ValidateValueType(string? value, CommandArgument argumentDefinition)
    {
        if (int.TryParse(value, out _))
        {
            return argumentDefinition.Type == typeof(int);
        }

        if (value is null || bool.TryParse(value, out _))
        {
            return argumentDefinition.Type == typeof(bool);
        }
        
        return argumentDefinition.Type == typeof(string);
    }
}