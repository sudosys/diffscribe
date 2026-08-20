using System.ClientModel;
using DiffScribe.Configuration;
using DiffScribe.Extensions;
using OpenAI;
using OpenAI.Chat;

namespace DiffScribe.AI;

public class OpenAiClient(ConfigHandler configHandler)
{
    /// <summary>
    /// Commit message generation does not benefit from reasoning, so it is turned off 
    /// to keep the generation as fast and as cheap as possible.
    /// </summary>
    /// <remarks>
    /// The reasoning effort surface of the OpenAI SDK is still marked as experimental, 
    /// hence the suppression. It has to be revisited when the SDK promotes it.
    /// </remarks>
#pragma warning disable OPENAI001
    private static readonly ChatCompletionOptions CompletionOptions = new()
    {
        ReasoningEffortLevel = ChatReasoningEffortLevel.None
    };
#pragma warning restore OPENAI001
    
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
        var client = GetClient();
        return SendMessage(client, message);
    }

    private ChatClient GetClient(string? apiKey = null) => 
        new(model: configHandler.Configuration.Llm.ParseApiName(),
            credential: new ApiKeyCredential(apiKey ?? configHandler.ReadApiKey()),
            options: new OpenAIClientOptions
            {
                NetworkTimeout = TimeSpan.FromSeconds(30)
            });
    
    private string SendMessage(ChatClient client, params ChatMessage[] message)
    {
        var completion = client.CompleteChat(message, CompletionOptions);

        return completion.Value.Content[0].Text;
    }
}
