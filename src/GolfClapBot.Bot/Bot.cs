using GolfClapBot.Domain.Configuration;
using Microsoft.Extensions.Options;
using OpenAI_API;

namespace GolfClapBot.Bot;

public class Bot : IBot
{
    private readonly OpenAIAPI _openAiClient;
    private static Data _data;
    public List<string> RepliedMessages { get; } = [];

    public Bot(IOptions<Data> data, IOptions<Settings> settings)
    {
        _data = data.Value;
        _openAiClient = new OpenAIAPI(settings.Value.OpenAi.ApiKey);
    }

    public async Task<string> AnalyzeChatMessage(string message, string username)
    {
        if (message.Length == 0)
            return string.Empty;

        var chat = _openAiClient.Chat.CreateConversation();

        _data.TrainingData?.ForEach(x => chat.AppendExampleChatbotOutput(x));

        chat.AppendUserInputWithName(username, message);
        RepliedMessages.Add(message);

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