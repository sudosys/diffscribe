using DiffScribe.Configuration;
using DiffScribe.Configuration.Enums;
using OpenAI.Chat;

namespace DiffScribe.AI;

public class CommitGenerator(OpenAiClient client, ConfigHandler configHandler)
{
    private readonly SystemChatMessage _commitGenInstruction 
        = new("Analyze the series of diffs for each staged file and generate a concise, relevant commit message following the specified commit message structure.");

    public string GenerateCommitMessage(string diffs)
    {
        var diffMessage = new UserChatMessage(diffs);
        ChatMessage[] messages = [_commitGenInstruction, GetCommitStructureInstruction(), diffMessage];
        return client.SendMessage(messages);
    }

    private SystemChatMessage GetCommitStructureInstruction() => 
        new($"Generate the commit title based on this structure: '{configHandler.Configuration.CommitStructure}'");
}