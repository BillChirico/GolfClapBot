namespace GolfClapBot.Runner.Configuration;

public class Settings
{
    public required Twitch Twitch { get; set; }

    public required OpenAI OpenAi { get; set; }
}