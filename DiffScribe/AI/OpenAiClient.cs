using DiffScribe.Configuration;
using OpenAI.Chat;

namespace DiffScribe.AI;

public class OpenAiClient(ConfigHandler configHandler)
{
    private readonly ToolConfiguration _config = configHandler.GetConfigurationFromFile();
    
    public bool TestApiKeyValidity(string? apiKey = null)
    {
        try
        {
            SendMessage(GetClient(apiKey), new SystemChatMessage("Just say 'hello'."));
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    private ChatClient GetClient(string? apiKey = null) => new(model: _config.Llm, apiKey: apiKey ?? _config.ApiKey);
 
    public string SendMessage(ChatClient client, ChatMessage message)
    {
        var completion = client.CompleteChat(message);

        return completion.Value.Content[0].Text;
    }
}