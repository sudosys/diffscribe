using DiffScribe.Commands;
using DiffScribe.Suggestions;

namespace DiffScribe;

public class CommandMatcher
{
    private readonly Dictionary<string, Lazy<ICommand>> _commandMappings;

    public CommandMatcher(IServiceProvider provider)
    {
        _commandMappings = new Dictionary<string, Lazy<ICommand>>
        {
            { string.Empty, new Lazy<ICommand>(() => new RootCommand(this)) },
            { "generate", new Lazy<ICommand>(() => new GenerateCommand(provider)) },
            { "config", new Lazy<ICommand>(() => new ConfigurationCommand(provider)) },
            { "reset", new Lazy<ICommand>(() => new ResetCommand(provider)) },
            { "help", new Lazy<ICommand>(() => new HelpCommand(this)) },
            { "version", new Lazy<ICommand>(() => new VersionCommand()) },
            { "update", new Lazy<ICommand>(() => new UpdateCommand(provider)) },
            { "uninstall", new Lazy<ICommand>(() => new UninstallCommand(provider)) },
        };
    }

    public bool TryMatch(string commandName, out ICommand? command)
    {
        if (GetMappedCommand(commandName, out var cmd))
        {
            command = cmd;
            return true;
        }
        
        command = null;
        return false;
    }

    private bool GetMappedCommand(string commandName, out ICommand? command)
    {
        switch (commandName.Length)
        {
            case 1:
            {
                var possibleKeys = _commandMappings.Keys
                    .Where(k => k.StartsWith(commandName))
                    .ToList();

                switch (possibleKeys.Count)
                {
                    case 0:
                        ReportUnknownCommand(commandName);
                        command = null;
                        return false;
                    case > 1:
                        ConsoleWrapper
                            .Error($"Ambiguous command: {commandName}. Possible matches are: {string.Join(", ", possibleKeys)}");
                        command = null;
                        return false;
                }
            
                return GetMappedCommand(possibleKeys.Single(), out command);
            }
            default:
                _commandMappings.TryGetValue(commandName, out var cmd);
                command = cmd?.Value;
                
                if (command is null)
                {
                    ReportUnknownCommand(commandName);
                    return false;   
                }
                return true;
        }
    }

    private void ReportUnknownCommand(string commandName)
    {
        ConsoleWrapper.Error($"Unknown command: {commandName}");

        var recommendation = SuggestionEngine.BuildRecommendation(commandName, GetDefinedCommandNames());

        ConsoleWrapper.Info(string.IsNullOrEmpty(recommendation)
            ? $"Run \"{AppInfo.ExecutableName} help\" to see the available commands."
            : recommendation);
    }
    
    public string[] GetDefinedCommandNames() 
        => _commandMappings.Keys
            .Where(k => k != string.Empty)
            .ToArray();
}
