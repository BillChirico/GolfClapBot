namespace Bapes.ChatBot.Worker;

public class Twitch
{
    public required string Channel { get; set; }

    public required string BotUser { get; set; }

    public required string OAuthToken { get; set; }
}