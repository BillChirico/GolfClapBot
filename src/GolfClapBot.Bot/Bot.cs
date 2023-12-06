using GolfClapBot.Domain.Configuration;
using Microsoft.Extensions.Options;
using OpenAI_API;

namespace GolfClapBot.Bot;

public class Bot : IBot
{
    private readonly OpenAIAPI _openAiClient;
    private static Data _data;

    public Bot(IOptions<Data> data, IOptions<Settings> settings)
    {
        _data = data.Value;
        _openAiClient = new OpenAIAPI(settings.Value.OpenAi.ApiKey);
    }

    public async Task<string> AnalyzeChatMessage(string message, string username)
    {
        var chat = _openAiClient.Chat.CreateConversation();

        _data.TrainingData?.ForEach(x => chat.AppendExampleChatbotOutput(x));

        if (message.Length == 0)
            return string.Empty;

        chat.AppendUserInputWithName(username, message);
        return await chat.GetResponseFromChatbotAsync();
    }
}