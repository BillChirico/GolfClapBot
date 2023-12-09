using GolfClapBot.Domain.Configuration;
using Microsoft.Extensions.Options;
using OpenAI_API;

namespace GolfClapBot.Bot;

public class Bot : IBot
{
    private readonly Settings settings;
    private readonly OpenAIAPI _openAiClient;
    private readonly Settings _settings;
    private static Data _data;

    public Bot(IOptions<Data> data, IOptions<Settings> settings)
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

    public Task<string> GetWelcomeMessage(string? version = "1.0.0")
    {
        var chat = _openAiClient.Chat.CreateConversation();

        chat.AppendUserInputWithName("Bapes",
            $"Create a message welcoming people to Bapes Twitch Stream. You are GolfClapBot a Twitch Chat Bot for Bapes. Be friendly and informative. You are on version {version} of GolfClapBot. Do not go over 250 characters.+");

        return chat.GetResponseFromChatbotAsync();
    }
}