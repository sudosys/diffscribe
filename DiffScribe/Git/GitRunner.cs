using System.Diagnostics;

namespace DiffScribe.Git;

public class GitRunner
{
    public bool IsGitInstalled()
    {
        using var process = Execute("git", "--version");
        
        var error = process?.StandardError.ReadToEnd();
        
        return string.IsNullOrEmpty(error);
    }

    public bool IsInsideGitRepository()
    {
        using var process = Execute("git", "rev-parse --is-inside-work-tree");
        
        var error = process?.StandardError.ReadToEnd();
        var output = process?.StandardOutput.ReadToEnd();
        
        return string.IsNullOrEmpty(error) && (output?.Contains("true") ?? false);
    }

    public bool StagedFilesPresent()
    {
        using var process = Execute("git", "diff --cached --name-only");
        
        var error = process?.StandardError.ReadToEnd();
        var output = process?.StandardOutput.ReadToEnd();

        return string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(output);
    }

    public string GetStagedDiffs()
    {
        using var process = Execute("git", "diff --cached");
        
        var output = process?.StandardOutput.ReadToEnd();
        
        return output ?? string.Empty;
    }

    public bool Commit(string commitMessage)
    {
        using var process = Execute("git", $"commit -m \"{commitMessage}\"");
        
        var error = process?.StandardError.ReadToEnd();
        
        return string.IsNullOrEmpty(error);
    }

    private Process? Execute(string command, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = string.Join(' ', arguments),
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        
        return Process.Start(startInfo);
    }
}