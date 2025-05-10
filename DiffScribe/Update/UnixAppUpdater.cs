using System.Diagnostics;

namespace DiffScribe.Update;

public class UnixAppUpdater : AppUpdater
{
    protected override string GetInstallationScript(string tempPath) 
        => Directory.GetFiles(tempPath, "*.sh", SearchOption.AllDirectories)[0];

    protected override async Task StartInstallation(string script)
    {
        using var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = "sh",
                Arguments = script,
                WorkingDirectory = Path.GetDirectoryName(script),
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
        
        var error = await process!.StandardError.ReadToEndAsync();
        if (!string.IsNullOrEmpty(error))
        {
            ConsoleWrapper.Error(error);
        }
        
        await process.WaitForExitAsync();
    }
}