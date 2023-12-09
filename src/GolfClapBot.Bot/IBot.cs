namespace GolfClapBot.Bot;

public interface IBot
{
    Task<string> AnalyzeChatMessage(string message, string username);

    Task<string> GetWelcomeMessage(string? version = "1.0.0");
}