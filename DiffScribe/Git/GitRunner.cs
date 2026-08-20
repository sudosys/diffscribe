using System.Diagnostics;

namespace DiffScribe.Git;

public class GitRunner
{
    private const string GitCommand = "git";
    
    public bool IsGitInstalled() => Execute("--version").Succeeded;

    public bool IsInsideGitRepository()
    {
        var result = Execute("rev-parse", "--is-inside-work-tree");
        
        return result.Succeeded && result.Output.Contains("true");
    }

    public bool StagedFilesPresent()
    {
        var result = Execute("diff", "--cached", "--name-only");
        
        return result.Succeeded && !string.IsNullOrWhiteSpace(result.Output);
    }

    public string GetStagedDiffs() => Execute("diff", "--cached").Output;

    public bool CommitsPresent() => Execute("rev-parse", "--verify", "--quiet", "HEAD").Succeeded;

    public string GetLastCommitMessage() => Execute("log", "-1", "--pretty=%B").Output.Trim();

    /// <summary>
    /// Diffs the amended commit would end up containing: the changes of the latest commit
    /// together with the extra staged changes.
    /// </summary>
    public string GetAmendDiffs()
    {
        if (ParentCommitPresent())
        {
            return Execute("diff", "--cached", "HEAD^").Output;
        }

        // The latest commit has no parent to diff the index against, 
        // so its own changes and the staged ones are gathered separately.
        var lastCommitDiffs = Execute("show", "--format=", "--patch", "HEAD").Output;
        
        return $"{lastCommitDiffs}{Environment.NewLine}{GetStagedDiffs()}";
    }

    public bool Commit(string commitMessage) => Execute("commit", "-m", commitMessage).Succeeded;

    public bool Amend(string commitMessage) => Execute("commit", "--amend", "-m", commitMessage).Succeeded;

    private bool ParentCommitPresent() => Execute("rev-parse", "--verify", "--quiet", "HEAD^").Succeeded;

    private GitResult Execute(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = GitCommand,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        try
        {
            using var process = Process.Start(startInfo);

            if (process is null)
            {
                return new GitResult(ExitCode: -1, Output: string.Empty, Error: $"{GitCommand} could not be started.");
            }

            // Error stream is drained concurrently to keep a full output stream from blocking the process.
            var errorTask = process.StandardError.ReadToEndAsync();
            var output = process.StandardOutput.ReadToEnd();
            var error = errorTask.GetAwaiter().GetResult();
            
            process.WaitForExit();

            return new GitResult(process.ExitCode, output, error);
        }
        catch (Exception e)
        {
            return new GitResult(ExitCode: -1, Output: string.Empty, Error: e.Message);
        }
    }

    private readonly record struct GitResult(int ExitCode, string Output, string Error)
    {
        public bool Succeeded => ExitCode == 0;
    }
}
