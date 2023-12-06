using Bapes.ChatBot.Worker.Configuration;
using Microsoft.Extensions.Options;
using OpenAI_API;

namespace Bapes.Chatbot.Worker;

public class AiBot
{
    private readonly Settings settings;
    public readonly OpenAIAPI _openAiClient;
    private readonly Settings _settings;
    private static Data _data;

    public AiBot(IOptions<Data> data, IOptions<Settings> settings)
    {
        _settings = settings.Value;
        _data = data.Value;
        _openAiClient = new OpenAIAPI(_settings.OpenAi.ApiKey);
    }

    public async Task<string> AnalyzeChatMessage(string message, string username)
    {
        var chat = _openAiClient.Chat.CreateConversation();

        _data.TrainingData?.ForEach(x => chat.AppendExampleChatbotOutput(x));

        if (message.Length == 0)
            return string.Empty;

        // var model = new Model("gpt-3.5-turbo-1106")
        // {
        //     ModelID = "ft:gpt-3.5-turbo-1106:volvox::8SYpeH7J"
        // };
        //
        // var result = await _openAiClient.Chat.CreateChatCompletionAsync(new ChatRequest
        // {
        //     Model = model,
        //     Temperature = 0.5,
        //     MaxTokens = 250,
        //     Messages = new[]
        //     {
        //         new ChatMessage(ChatMessageRole.User, message)
        //     }
        // });

        chat.AppendUserInputWithName(username, message);
        return await chat.GetResponseFromChatbotAsync();
    }
}