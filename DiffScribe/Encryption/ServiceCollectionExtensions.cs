using Microsoft.Extensions.DependencyInjection;

namespace DiffScribe.Encryption;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterSecretKeyHandlers(this IServiceCollection services)
    {
        services.AddSingleton<WindowsSecretKeyHandler>();
        services.AddSingleton<UnixSecretKeyHandler>();

        services.AddSingleton<SecretKeyHandler>(sp =>
        {
            if (OperatingSystem.IsWindows())
            {
                return sp.GetRequiredService<WindowsSecretKeyHandler>();
            }
            
            return sp.GetRequiredService<UnixSecretKeyHandler>();
        });
        
        return services;
    }
}