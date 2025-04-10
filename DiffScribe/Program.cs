using DiffScribe.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace DiffScribe;

class Program
{
    static void Main(string[] args)
    {
        var serviceCollection = RegisterServices();
        
        var commandInfo = CommandParser.Parse(args);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commandDispatcher = serviceProvider.GetRequiredService<CommandDispatcher>();
        
        commandDispatcher.Dispatch(commandInfo);
    }

    private static ServiceCollection RegisterServices()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton<CommandParser>()
            .AddSingleton<CommandDispatcher>();
        
        return serviceCollection;
    }
}