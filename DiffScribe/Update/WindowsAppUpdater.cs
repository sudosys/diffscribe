using System.Diagnostics;

namespace DiffScribe.Update;

public class WindowsAppUpdater : AppUpdater
{
    protected override string InstallationScriptName => "install.ps1";

    protected override Task StartInstallation(string script)
    {
        using var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-ExecutionPolicy Bypass -File \"{script}\"",
                UseShellExecute = true
            });
        
        ConsoleWrapper.Info("Installation script has been started. Exiting application...");
        Environment.Exit(0);
        
        return Task.CompletedTask;
    }
}