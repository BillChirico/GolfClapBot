namespace GolfClapBot.Bot;

public interface IBot
{
    Task<string> AnalyzeChatMessage(string message, string username);
}