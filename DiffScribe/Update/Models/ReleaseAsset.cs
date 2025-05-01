using System.Text.Json.Serialization;

namespace DiffScribe.Update.Models;

public class ReleaseAsset
{
    [JsonPropertyName("url")]
    public string DownloadUrl { get; set; }
}