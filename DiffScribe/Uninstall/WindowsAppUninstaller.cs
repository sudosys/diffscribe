using System.Diagnostics;
using DiffScribe.Configuration;

namespace DiffScribe.Uninstall;

public class WindowsAppUninstaller(ConfigHandler configHandler) : IAppUninstaller
{
    public string UninstallScriptPath => $"{AppContext.BaseDirectory}uninstall.ps1";
    
    public void Uninstall()
    {
        configHandler.RemoveConfiguration();
        Process.Start(GetProcessStartInfo());
        
        ConsoleWrapper.Info("Uninstallation script has been started. Exiting application...");
        Environment.Exit(0);
    }
    
    public ProcessStartInfo GetProcessStartInfo() =>
        new()
        {
            FileName = "powershell",
            Arguments = $"-ExecutionPolicy Bypass -File \"{UninstallScriptPath}\"",
            UseShellExecute = true
        };
}