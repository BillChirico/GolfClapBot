namespace GolfClapBot.Domain.Configuration;

public class OpenAISettings
{
    public required string ApiKey { get; set; }

    public required string Model { get; set; }
}