using Bapes.ChatBot.Worker;
using Bapes.ChatBot.Worker.Configuration;
using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

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

        chat.AppendUserInput("Do you also stream on Kick?");
        chat.AppendExampleChatbotOutput(
            "Yes! You can find my Kick at https://kick.com/BapesGolfClap also known as @BapesGolfClap");

        chat.AppendUserInput("Socials");
        chat.AppendUserInput("Do you have any socials?");
        chat.AppendExampleChatbotOutput(
            "We have TikTok @BapesGolfClap, Kick kick.tv/BapesGolfClap, & YouTube @BapesGolfClap");

        chat.AppendUserInput("What else can you do?");
        chat.AppendExampleChatbotOutput(
            "I can answer question based on the stream, Bapes, and just about anything else you can think of!");

        chat.AppendUserInput("What is Kick?");
        chat.AppendExampleChatbotOutput(
            "Kick is a live streaming platform where you can watch and interact with streamers.");

        chat.AppendUserInput("What song is this?");
        chat.AppendUserInput("Song?");
        chat.AppendExampleChatbotOutput(
            "We are currently listening to a DMCA Free music playlist on Spotify. You can find the playlist here: ");

        if (message.Length == 0)
            return string.Empty;


        var result = await _openAiClient.Chat.CreateChatCompletionAsync(new ChatRequest
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.1,
            MaxTokens = 50,
            Messages = new ChatMessage[]
            {
                new(ChatMessageRole.User, message)
            }
        });

        chat.AppendUserInputWithName(username, message);
        return await chat.GetResponseFromChatbotAsync();
    }
}