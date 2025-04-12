using DiffScribe.Configuration;
using OpenAI.Chat;

namespace DiffScribe.AI;

public class OpenAiClient(ConfigHandler configHandler)
{
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

    public string SendMessage(params ChatMessage[] message)
    {
        var client = GetClient(apiKey: null);
        return SendMessage(client, message);
    }
    
    private string SendMessage(ChatClient client, params ChatMessage[] message)
    {
        var completion = client.CompleteChat(message);

        return completion.Value.Content[0].Text;
    }

    private ChatClient GetClient(string? apiKey = null) => 
        new(model: configHandler.Configuration.Llm, apiKey: apiKey ?? configHandler.Configuration.ApiKey);
}