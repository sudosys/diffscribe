using Microsoft.Extensions.DependencyInjection;

namespace DiffScribe.Uninstall;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterUninstallers(this IServiceCollection services)
    {
        services.AddSingleton<WindowsAppUninstaller>();
        services.AddSingleton<UnixAppUninstaller>();

        services.AddSingleton<AppUninstaller>(sp =>
        {
            if (OperatingSystem.IsWindows())
            {
                return sp.GetRequiredService<WindowsAppUninstaller>();
            }
            
            return sp.GetRequiredService<UnixAppUninstaller>();
        });
        
        return services;
    }
}