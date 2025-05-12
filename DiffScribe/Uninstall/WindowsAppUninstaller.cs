using System.Diagnostics;
using DiffScribe.Configuration;
using DiffScribe.Encryption;

namespace DiffScribe.Uninstall;

public class WindowsAppUninstaller(ConfigHandler configHandler, SecretKeyHandler secretKeyHandler)
    : AppUninstaller(configHandler, secretKeyHandler)
{
    protected override string UninstallScriptPath => $"{AppContext.BaseDirectory}uninstall.ps1";
    
    public override void Uninstall()
    {
        base.Uninstall();
        
        Process.Start(GetProcessStartInfo());
        
        ConsoleWrapper.Info("Uninstallation script has been started. Exiting application...");
        Environment.Exit(0);
    }
    
    protected override ProcessStartInfo GetProcessStartInfo() =>
        new()
        {
            FileName = "powershell",
            Arguments = $"-ExecutionPolicy Bypass -File \"{UninstallScriptPath}\"",
            UseShellExecute = true
        };
}