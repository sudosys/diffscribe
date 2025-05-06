using System.Diagnostics;
using DiffScribe.Commands.Models;

namespace DiffScribe.Commands;

public class UninstallCommand : ICommand
{
    public string Name => "uninstall";
    
    public string Description => "Uninstalls the CLI tool.";

    public CommandArgument[] DefinedArguments => [];
    
    public void Execute(Dictionary<string, object?> args)
    {
        var proceed = ConsoleWrapper.ShowConfirmation($"Are you sure you want to uninstall {AppInfo.Name}?");
        
        if (!proceed)
        {
            ConsoleWrapper.Info("Operation aborted.");
            return;
        }

        ConsoleWrapper.Info("Uninstallation has been started...");

        var process = RunScript(GetScriptPath());
        
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

    private string GetScriptPath() => $"{AppContext.BaseDirectory}uninstall{ResolveScriptExtension()}";

    private string ResolveScriptExtension() => OperatingSystem.IsWindows() ? ".ps1" : ".sh";

    private Process? RunScript(string scriptPath) =>
        Process.Start(new ProcessStartInfo
        {
            FileName = scriptPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });
}