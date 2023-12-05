using Microsoft.Extensions.Options;
using OpenAI_API;

namespace Bapes.Chatbot.Worker;

public class AiBot
{
    private readonly Data _data;
    private readonly OpenAIAPI _openAiClient;

    public AiBot(IOptions<OpenAI> apiKey, IOptions<Data> data)
    {
        _data = data.Value;
        _openAiClient = new OpenAIAPI("key");
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
        chat.AppendExampleChatbotOutput("I can answer questionsdd based on the stream,");

        chat.AppendUserInput("What is Kick?");
        chat.AppendExampleChatbotOutput(
            "Kick is a live streaming platform where you can watch and interact with streamers.");

        chat.AppendUserInput("What song is this?");
        chat.AppendUserInput("Song?");
        chat.AppendExampleChatbotOutput(
            "We are currently listening to a DMCA Free music playlist on Spotify. You can find the playlist here: ");

        if (message.Length == 0)
            return string.Empty;

        chat.AppendUserInputWithName(username, message);
        return await chat.GetResponseFromChatbotAsync();
    }
}