using System.Text.Json.Serialization;

namespace DiffScribe.Update.Models;

public class ReleaseAsset
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("url")]
    public string DownloadUrl { get; set; } = string.Empty;
}