namespace DiffScribe.Configuration;

public class ToolConfiguration(string commitStyle, bool autoCommit, string apiKey, string llm)
{
    public string CommitStyle { get; set; } = commitStyle;
    
    public bool AutoCommit { get; set; } = autoCommit;
    
    public string ApiKey { get; set; } = apiKey;
 
    public string Llm { get; set; } = llm;
}