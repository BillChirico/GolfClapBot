using System.Reflection;
using Bapes.Chatbot.Worker;
using Microsoft.Extensions.Options;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace Bapes.ChatBot.Worker;

public class Worker(ILogger<Worker> logger, IOptions<Data> restrictedPhrases, AiBot bot, ILoggerFactory loggerFactory)
    : BackgroundService
{
    private readonly TwitchClient _client = new(loggerFactory: loggerFactory);

    private async Task SendMessage(string message, string username = "")
    {
        logger.LogInformation("Bot is sending message: {Message} in channel {Username}", message, username);

        await _client.SendMessageAsync("Bapes", message);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var credentials = new ConnectionCredentials("GolfClapBot", "key");

        _client.Initialize(credentials, "Bapes");

        _client.OnJoinedChannel += ClientOnOnJoinedChannel;
        _client.OnMessageReceived += ClientOnOnMessageReceived;

        await _client.ConnectAsync();

        logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
    }

    private async Task ClientOnOnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.DisplayName == "GolfClapBot")
            return;

        if (e.ChatMessage.Message.Length == 0)
            return;

        if (string.IsNullOrEmpty(e.ChatMessage.Message))
            return;

        var response = await bot.AnalyzeChatMessage(e.ChatMessage.Message, e.ChatMessage.Username);

        if (restrictedPhrases.Value.RestrictedPhrases != null && restrictedPhrases.Value.RestrictedPhrases.Exists(
                restrictedPhrase =>
                    response.Contains(restrictedPhrase.ToLower(), StringComparison.InvariantCultureIgnoreCase)))
        {
            return;
        }

        await SendMessage(response, e.ChatMessage.Username);
    }

    private async Task ClientOnOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Console.WriteLine($"Connected to {e.Channel}");
        await SendMessage(
            $"Hello! I'm GolfClapBot, the AI chat bot developed by Bapes. If you're interested in learning more about it, please feel free to message him! v{GetVersion()}");
    }

    private static string? GetVersion()
    {
        return typeof(Worker).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
    }

    // private static string RemoveEmotes(ChatMessage message)
    // {
    //     StringBuilder parsed = new(message.Message);
    //
    //     foreach (var emote in message.EmoteSet.Emotes.OrderByDescending(x => x.StartIndex))
    //     {
    //         parsed.Remove(emote.StartIndex, emote.EndIndex - emote.StartIndex + 1);
    //         parsed.Replace("  ", " ");
    //     }
    //
    //     return parsed.ToString();
    // }
}