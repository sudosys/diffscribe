using System.Diagnostics;
using DiffScribe.Configuration;

namespace DiffScribe.Uninstall;

public class UnixAppUninstaller(ConfigHandler configHandler) : IAppUninstaller
{
    public string UninstallScriptPath => $"{AppContext.BaseDirectory}uninstall.sh";
    
    public void Uninstall()
    {
        var process = Process.Start(GetProcessStartInfo());
        configHandler.RemoveConfiguration();
        
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
    
    public ProcessStartInfo GetProcessStartInfo() =>
        new()
        {
            FileName = UninstallScriptPath,
            Arguments = string.Empty,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
}