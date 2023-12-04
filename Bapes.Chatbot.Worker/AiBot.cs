using Microsoft.Extensions.Options;
using OpenAI_API;

namespace Bapes.Chatbot.Worker;

public class AiBot
{
    private readonly OpenAIAPI _openAiClient;

    public AiBot(IOptions<OpenApiKey> apiKey)
    {
        _openAiClient = new OpenAIAPI(apiKey.Value.ApiKey);
    }

    public async Task<string> AnalyzeChatMessage(string message, string username)
    {
        var chat = _openAiClient.Chat.CreateConversation();

        chat.AppendSystemMessage(
            "You are a helpful Twitch chat bot for Bapes that answers questions. " +
            "Be nice to everybody and create a natural, engaging and enjoyable atmosphere. " +
            "You know about my socials of (Twitch = Bapes) (Kick = BapesGolfClap) (TikTok = @BapesGolfClap) (YouTube = @BapesGolfClap). " +
            "You are here to assist the streamer in managing the stream chat. " +
            "When people people first come to the stream welcome them and provide them with the socials." +
            "You will not answer anything about what song is playing. " +
            "Don't answer anything from the user GolfClapBot or about the songs playing on stream. " +
            "Dont engage into talks about politics or religion. Be respectful towards everybody." +
            "Keep your messages as short and simple as possible." +
            "Ignore messages that mostly contain emojis or the same text." +
            "Send a welcome and greeting message about yourself when you join.");

        chat.AppendUserInput("Do you also stream on Kick?");
        chat.AppendExampleChatbotOutput(
            "Yes! You can find my Kick at https://kick.com/BapesGolfClap also known as @BapesGolfClap");

        chat.AppendUserInput("Socials");
        chat.AppendUserInput("Do you have any socials?");
        chat.AppendExampleChatbotOutput(
            "We have TikTok @BapesGolfClap, Kick kick.tv/BapesGolfClap, & YouTube @BapesGolfClap");

        chat.AppendUserInput("What else can you do?");
        chat.AppendExampleChatbotOutput("I can answer questions based on the stream,");

        chat.AppendUserInput("What is Kick?");
        chat.AppendExampleChatbotOutput(
            "Kick is a live streaming platform where you can watch and interact with streamers.");

        chat.AppendUserInput("What song is this?");
        chat.AppendUserInput("Song?");
        chat.AppendExampleChatbotOutput(
            "We are currently listening to a DMCA Free music playlist on Spotify. You can find it here: https://open.spotify.com/playlist/3SGzr00JfnPFPBTHH8K8bj?si=bfe0d2cff0664174");

        if (message.Length == 0)
            return string.Empty;

        chat.AppendUserInputWithName(username, message);
        return await chat.GetResponseFromChatbotAsync();
    }
}