namespace GolfClapBot.Domain.Configuration;

public class OpenAiSettings
{
    public required string ApiKey { get; set; }

    public required string Model { get; set; }
}