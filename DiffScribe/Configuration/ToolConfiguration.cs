namespace DiffScribe.Configuration;

public class ToolConfiguration(string commitStructure, bool autoCommit, string apiKey, string llm)
{
    public string CommitStructure { get; set; } = commitStructure;
    
    public bool AutoCommit { get; set; } = autoCommit;
    
    public string ApiKey { get; set; } = apiKey;
 
    public string Llm { get; set; } = llm;
}