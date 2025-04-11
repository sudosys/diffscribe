namespace DiffScribe.Configuration;

public class ToolConfiguration(string commitStructure, bool autoCommit, bool autoPush, string apiKey, string llm)
{
    public string CommitStructure { get; set; } = commitStructure;
    
    public bool AutoCommit { get; set; } = autoCommit;
    
    public bool AutoPush { get; set; } = autoPush;
    
    public string ApiKey { get; set; } = apiKey;
 
    public string Llm { get; set; } = llm;
}