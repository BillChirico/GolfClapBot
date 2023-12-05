using Bapes.Chatbot.Worker;

namespace Bapes.ChatBot.Worker;

public class Settings
{
    public required Twitch Twitch { get; set; }

    public required OpenAI OpenAi { get; set; }
}