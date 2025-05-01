using DiffScribe.AI;
using DiffScribe.Configuration;
using DiffScribe.Encryption;
using DiffScribe.Git;
using DiffScribe.Parser;
using DiffScribe.Update;
using Microsoft.Extensions.DependencyInjection;

namespace DiffScribe;

class Program
{
    public static async Task Main(string[] args)
    {
        SetConsoleSettings();
        
        var serviceCollection = RegisterServices();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        await TryInstallUpdate(serviceProvider);

        var commandParser = serviceProvider.GetRequiredService<CommandParser>();
        var commandInfo = commandParser.Parse(args);
        
        var commandDispatcher = serviceProvider.GetRequiredService<CommandDispatcher>();
        commandDispatcher.Dispatch(commandInfo);
    }

    private static void SetConsoleSettings()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
    }

    private static ServiceCollection RegisterServices()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton<CommandParser>()
            .AddSingleton<CommandDispatcher>()
            .AddSingleton<CommandMatcher>()
            .AddSingleton<ArgumentValidator>()
            .AddSingleton<ConfigHandler>()
            .AddSingleton<GitRunner>()
            .AddScoped<OpenAiClient>()
            .AddSingleton<CommitGenerator>()
            .AddSingleton<EncryptionService>()
            .AddSingleton<AppUpdater>();
        
        return serviceCollection;
    }
    
    private static async Task TryInstallUpdate(IServiceProvider provider)
    {
        var appUpdater = provider.GetRequiredService<AppUpdater>();

        var url = await appUpdater.TryGetNewVersionUrl();

        if (url != null)
        {
            await appUpdater.DownloadInstallUpdate(url);
        }
    }
}