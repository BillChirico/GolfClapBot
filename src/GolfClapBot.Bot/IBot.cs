namespace GolfClapBot.Bot;

public interface IBot
{
    List<string> RepliedMessages { get; }

    Task<string> AnalyzeChatMessage(string message, string username);

    Task<string> GetWelcomeMessage(string? version = "1.0.0");
}