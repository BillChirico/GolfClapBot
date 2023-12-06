namespace Bapes.ChatBot.Worker.Configuration;

public class OpenAI
{
    public required string ApiKey { get; set; }

    public required string Model { get; set; }
}