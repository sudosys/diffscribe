using Microsoft.Extensions.DependencyInjection;

namespace DiffScribe.Update;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterUpdaters(this IServiceCollection services)
    {
        services.AddSingleton<WindowsAppUpdater>();
        services.AddSingleton<UnixAppUpdater>();

        services.AddSingleton<AppUpdater>(sp =>
        {
            if (OperatingSystem.IsWindows())
            {
                return sp.GetRequiredService<WindowsAppUpdater>();
            }
            
            return sp.GetRequiredService<UnixAppUpdater>();
        });
        
        return services;
    }
}