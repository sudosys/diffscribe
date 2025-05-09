using DiffScribe.Commands.Models;
using DiffScribe.Uninstall;
using Microsoft.Extensions.DependencyInjection;

namespace DiffScribe.Commands;

public class UninstallCommand(IServiceProvider provider) : ICommand
{
    public string Name => "uninstall";
    
    public string Description => "Uninstalls the CLI tool.";

    public CommandArgument[] DefinedArguments => [];
    
    private readonly IAppUninstaller _uninstaller = provider.GetRequiredService<IAppUninstaller>();
    
    public void Execute(Dictionary<string, object?> args)
    {
        var proceed = ConsoleWrapper.ShowConfirmation($"Are you sure you want to uninstall {AppInfo.Name}?");
        
        if (!proceed)
        {
            ConsoleWrapper.Info("Operation aborted.");
            return;
        }

        ConsoleWrapper.Info("Uninstallation has been started...");

        _uninstaller.Uninstall();
    }
}