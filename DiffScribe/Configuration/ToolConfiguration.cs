using DiffScribe.Configuration.Enums;

namespace DiffScribe.Configuration;

public class ToolConfiguration(
    string commitStyle,
    bool autoCommit,
    string apiKey,
    string llm,
    string? commitLength = null)
{
    public string CommitStyle { get; set; } = commitStyle;
    
    /// <summary>
    /// Either a <see cref="Enums.CommitLength"/> preset name or a custom character count.
    /// Configurations written before this setting existed fall back to the default preset.
    /// </summary>
    public string CommitLength { get; set; } = commitLength ?? nameof(Enums.CommitLength.Standard);
    
    public bool AutoCommit { get; set; } = autoCommit;
    
    public string ApiKey { get; set; } = apiKey;
 
    public string Llm { get; set; } = llm;
}
