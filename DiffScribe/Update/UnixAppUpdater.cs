using System.Diagnostics;

namespace DiffScribe.Update;

public class UnixAppUpdater : AppUpdater
{
    protected override string InstallationScriptName => "install.sh";

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