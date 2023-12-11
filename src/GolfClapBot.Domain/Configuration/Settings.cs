namespace GolfClapBot.Domain.Configuration;

public class Settings
{
    public required TwitchSettings TwitchSettings { get; set; }

    public required OpenAISettings OpenAiSettings { get; set; }
}