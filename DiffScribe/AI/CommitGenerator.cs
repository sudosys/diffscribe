using DiffScribe.Configuration;
using DiffScribe.Configuration.Enums;
using OpenAI.Chat;

namespace DiffScribe.AI;

public class CommitGenerator(OpenAiClient client, ConfigHandler configHandler)
{
    private const string CommitGenerationInstruction = """
                         Analyze the series of diffs for each staged file and generate a concise, 
                         relevant commit message complying to the 'Conventional Commits Specification'.
                         
                         Example structure: <type>[optional scope]: <description>
                         """;
    
    private readonly SystemChatMessage _commitGenInstruction 
        = new(CommitGenerationInstruction);

    public string GenerateCommitMessage(string diffs)
    {
        var diffMessage = new UserChatMessage(diffs);
        ChatMessage[] messages = [_commitGenInstruction, CreateCommitStyleInstruction(), diffMessage];
        return client.SendMessage(messages);
    }

    private SystemChatMessage CreateCommitStyleInstruction()
    {
        if (!Enum.TryParse(configHandler.Configuration.CommitStyle, out CommitStyle style))
        {
            ConsoleWrapper.Warning($"The commit style \"{configHandler.Configuration.CommitStyle}\" could not be parsed.");
            ConsoleWrapper.Info("Proceeding with the default commit style.");
            
            style = CommitStyle.Standard;
        }

        var instruction = style switch
        {
            CommitStyle.Minimal => "Generate a minimal commit message. Do NOT include any body or footer(s) and keep the description as short as possible.",
            CommitStyle.Standard => "Generate a standard commit message. Do NOT include any body or footer(s).",
            CommitStyle.Detailed => "Generate a detailed commit message. Include body and/or footer(s) if applicable in the context.",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        return new SystemChatMessage(instruction);
    }
}