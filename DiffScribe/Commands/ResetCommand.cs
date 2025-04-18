using DiffScribe.Commands.Models;
using DiffScribe.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiffScribe.Commands;

public class ResetCommand(IServiceProvider provider) : ICommand
{
    public string Name => "reset";

    public string Description => "Resets the application's configuration.";

    public CommandArgument[] DefinedArguments => [];
    
    private readonly ConfigHandler _configHandler = provider.GetRequiredService<ConfigHandler>();
    
    public void Execute(Dictionary<string, object?> args)
    {
        var proceed = ConsoleWrapper
            .ShowConfirmation("All configuration including your API key will be reset. Are you sure you want to continue?");

        if (!proceed)
        {
            ConsoleWrapper.Info("Operation aborted.");
            return;
        }

        try
        {
            _configHandler.ResetConfiguration();
        }
        catch (Exception e)
        {
            ConsoleWrapper.Error(e.Message);
            ConsoleWrapper.Info("Configuration could not be reset.");
            throw;
        }
        
        ConsoleWrapper.Success("Configuration has been reset to default. API key must be set in order to use the tool.");
    }
}