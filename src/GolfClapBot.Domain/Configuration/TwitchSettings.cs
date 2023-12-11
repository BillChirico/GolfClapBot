namespace GolfClapBot.Domain.Configuration;

public class TwitchSettings
{
    public required string Channel { get; set; }

    public required string BotUser { get; set; }

    public required string OAuthToken { get; set; }

    public required string ClientId { get; set; }
}