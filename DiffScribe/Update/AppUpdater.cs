using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using DiffScribe.Update.Models;

namespace DiffScribe.Update;

public class AppUpdater
{
    private const char VersionPrefix = 'v';

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

        var shFile = GetInstallationScript(tempPath);
        await StartInstallation(shFile);
        
        ConsoleWrapper.Success("New version has been installed successfully.");
    }

    private string CreateTempDirectory()
    {
        var extractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(extractPath);
        return extractPath;
    }

    private string GetInstallationScript(string tempPath) 
        => Directory.GetFiles(tempPath, "*.sh", SearchOption.AllDirectories)[0];

    private async Task StartInstallation(string shFile)
    {
        using var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = "sh",
                ArgumentList = { shFile },
                WorkingDirectory = Path.GetDirectoryName(shFile),
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
        
        var error = await process!.StandardError.ReadToEndAsync();
        ConsoleWrapper.Error(error);
        
        await process.WaitForExitAsync();
    }
    #endregion

    public async Task<string?> TryGetNewVersionUrl()
    {
        var latestRelease = await GetLatestRelease();

        if (latestRelease != null && CompareLatestWithCurrentVersion(latestRelease.TagName))
        {
            ConsoleWrapper.Info("New version of DiffScribe available!");
            return latestRelease.Assets[0].DownloadUrl;
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
    
    private bool CompareLatestWithCurrentVersion(string latestVersionTag)
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
}