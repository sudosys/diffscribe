using DiffScribe.AI;
using DiffScribe.Configuration;
using DiffScribe.Git;
using DiffScribe.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace DiffScribe;

class Program
{
    public static void Main(string[] args)
    {
        var serviceCollection = RegisterServices();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var commandParser = serviceProvider.GetRequiredService<CommandParser>();
        var commandInfo = commandParser.Parse(args);

        var commandDispatcher = serviceProvider.GetRequiredService<CommandDispatcher>();
        commandDispatcher.Dispatch(commandInfo);
    }

    private static ServiceCollection RegisterServices()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton<CommandParser>()
            .AddSingleton<CommandDispatcher>()
            .AddSingleton<ArgumentValidator>()
            .AddSingleton<ConfigHandler>()
            .AddSingleton<GitRunner>()
            .AddScoped<OpenAiClient>();
        
        return serviceCollection;
    }
}