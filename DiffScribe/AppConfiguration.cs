using Microsoft.Extensions.Configuration;
namespace DiffScribe;

public class AppConfiguration
{
    private const string ConfigFileName = "appsettings.json";

    public IConfiguration Configuration => new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(ConfigFileName, optional: false, reloadOnChange: false)
        .Build();
}