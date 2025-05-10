using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using DiffScribe.Update.Models;

namespace DiffScribe.Update;

public abstract class AppUpdater
{
    private const char VersionPrefix = 'v';

    protected abstract string InstallationScriptName { get; }
    
    private readonly string[] _updateIgnoredCommands =
    [
        "update",
        "uninstall",
        "version"
    ];

    public async Task CheckForUpdates(string commandToBeExecuted)
    {
        if (_updateIgnoredCommands.Contains(commandToBeExecuted))
        {
            return;
        }
        
        var url = await TryGetNewVersionUrl();

        if (url != null)
        {
            await DownloadInstallUpdate(url);
        }
    }

    #region Update Downloading
    public async Task DownloadInstallUpdate(string downloadUrl)
    {
        ConsoleWrapper.Info("Downloading the new version.");

        using var client = GetHttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");
        
        using var response = await client.GetAsync(downloadUrl);
        
        var zipPath = Path.GetTempFileName();
        await using (var stream = File.Create(zipPath))
        {
            await response.Content.CopyToAsync(stream);
        }

        var tempPath = CreateTempDirectory();
        ZipFile.ExtractToDirectory(zipPath, tempPath);
        
        ConsoleWrapper.Info("Download finished, installing the new version.");

        var scriptFile = GetInstallationScript(tempPath);
        if (scriptFile is null)
        {
            ConsoleWrapper.Error("Installation script could not be found. Aborting update...");
            return;
        }
        
        await StartInstallation(scriptFile);
        
        ConsoleWrapper.Success("New version has been installed successfully.");
        
        DoFileCleanup(zipPath, tempPath);
    }

    private string CreateTempDirectory()
    {
        var extractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(extractPath);
        return extractPath;
    }

    private string? GetInstallationScript(string tempPath) 
        => Directory.GetFiles(tempPath, InstallationScriptName, SearchOption.AllDirectories).SingleOrDefault();

    protected abstract Task StartInstallation(string script);

    private void DoFileCleanup(params string[] paths)
    {
        foreach (var path in paths)
        {
            try
            {
                Directory.Delete(Path.GetFullPath(path), true);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
    #endregion

    #region Get Latest Version
    public async Task<string?> TryGetNewVersionUrl()
    {
        var latestRelease = await GetLatestRelease();

        if (latestRelease != null && IsLatestVersionNewerThanCurrent(latestRelease.TagName))
        {
            ConsoleWrapper.Info("New version of DiffScribe available!");
            return ResolveAssetDownloadUrl(latestRelease);
        }

        return null;
    }

    private async Task<AppRelease?> GetLatestRelease()
    {
        using var client = GetHttpClient();

        string response;
        try
        {
            response = await client.GetStringAsync(GetLatestReleaseEndpointUrl());
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (HttpRequestException e)
        {
            ConsoleWrapper.Error(e.Message);
            return null;
        }
        
        return JsonSerializer.Deserialize<AppRelease>(response);
    }

    private HttpClient GetHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(AppInfo.Name);
        return client;
    }

    private static string GetLatestReleaseEndpointUrl() 
        => $"https://api.github.com/repos/{AppInfo.OwnerUsername}/{AppInfo.Name.ToLower()}/releases/latest";
    
    private bool IsLatestVersionNewerThanCurrent(string latestVersionTag)
    {
        var parsedLatestVersion = ParseVersion(latestVersionTag);
        var parsedCurrentVersion = ParseVersion(AppInfo.Version);
        
        return parsedLatestVersion > parsedCurrentVersion;
    }

    private int ParseVersion(string version)
    {
        var concatVersion = string.Join("", version.Trim(VersionPrefix).Split('.'));
        return int.TryParse(concatVersion, out var versionNumber) ? versionNumber : 0;
    }

    private string? ResolveAssetDownloadUrl(AppRelease release)
    {
        var assetUrl = release.Assets
            .SingleOrDefault(a => a.Name[..a.Name.LastIndexOf('.')].EndsWith(RuntimeInformation.RuntimeIdentifier))
            ?.DownloadUrl;

        if (assetUrl is null)
        {
            ConsoleWrapper.Error("Could not find a suitable version for your system.");
            return null;
        }
        
        return assetUrl;
    }
    #endregion
}