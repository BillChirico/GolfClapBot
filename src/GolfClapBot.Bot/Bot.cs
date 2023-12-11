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

    /// <summary>
    ///     Analyzes a chat message sent by a user.
    /// </summary>
    /// <param name="message">The message sent by the user.</param>
    /// <param name="username">The username of the user.</param>
    /// <returns>Returns the response from the chatbot.</returns>
    /// <remarks>
    ///     The method creates a conversation using the OpenAiClient and appends example chatbot outputs from the training
    ///     data, if any.
    ///     It then appends the user's input with the specified username and stores the replied messages.
    ///     Finally, it retrieves the response from the chatbot.
    /// </remarks>
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

    /// <summary>
    ///     Retrieves a welcome message for Bapes Twitch Stream.
    /// </summary>
    /// <param name="version">The version of GolfClapBot (optional, default value is "1.0.0").</param>
    /// <returns>A task representing the asynchronous operation. The result is a string representing the welcome message.</returns>
    public Task<string> GetWelcomeMessage(string? version = "1.0.0")
    {
        var chat = _openAiClient.Chat.CreateConversation();

        chat.AppendUserInputWithName("Bapes",
            $"Create a message welcoming people to Bapes Twitch Stream. You are GolfClapBot a Twitch Chat Bot for Bapes. Be friendly, informative, silly, and sarcastic. You are on version {version} of GolfClapBot. Do not go over 250 characters.+");

        return chat.GetResponseFromChatbotAsync();
    }
}