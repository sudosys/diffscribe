using System.Text.Json.Serialization;

namespace DiffScribe.Update.Models;

public class AppRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; init; }

    [JsonPropertyName("assets")]
    public List<ReleaseAsset> Assets { get; set; }
}
