using System.Diagnostics;
using DiffScribe.Configuration;
using DiffScribe.Encryption;

namespace DiffScribe.Uninstall;

public class UnixAppUninstaller(ConfigHandler configHandler, SecretKeyHandler secretKeyHandler)
    : AppUninstaller(configHandler, secretKeyHandler)
{
    protected override string UninstallScriptPath => $"{AppContext.BaseDirectory}uninstall.sh";
    
    public override void Uninstall()
    {
        base.Uninstall();
        
        var process = Process.Start(GetProcessStartInfo());
        
        var output = process?.StandardOutput.ReadToEnd();
        var error = process?.StandardError.ReadToEnd();
        
        if (string.IsNullOrEmpty(error))
        {
            Console.WriteLine(output);
            ConsoleWrapper.Success("Uninstallation was successful.");
        }
        else
        {
            ConsoleWrapper.Error(error);
            ConsoleWrapper.Info("Uninstallation completed with errors. Please check the output above for more details.");
        }
    }
    
    protected override ProcessStartInfo GetProcessStartInfo() =>
        new()
        {
            FileName = UninstallScriptPath,
            Arguments = string.Empty,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
}