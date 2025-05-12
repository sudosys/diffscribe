using DiffScribe.Commands.Models;
using DiffScribe.Update;
using Microsoft.Extensions.DependencyInjection;

namespace DiffScribe.Commands;

public class UpdateCommand(IServiceProvider provider) : ICommand
{
    public string Name => "update";
    
    public string Description => "Checks for an available update and downloads it.";
    
    public CommandArgument[] DefinedArguments => [];
    
    private readonly AppUpdater _appUpdater = provider.GetRequiredService<AppUpdater>();
    
    public void Execute(Dictionary<string, object?> args)
    {
        Task.Run(async () => await ExecuteUpdate()).Wait();
    }

    private async Task ExecuteUpdate()
    {
        var url = await _appUpdater.TryGetNewVersionUrl();

        if (url is null)
        {
            ConsoleWrapper.Info("There is no update available at the moment.");
            return;
        }

        await _appUpdater.DownloadInstallUpdate(url);
    }
}