using System.Globalization;
using System.Reflection;
using System.Text;
using Bapes.Chatbot.Worker;
using Microsoft.Extensions.Options;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace Bapes.ChatBot.Worker;

public class Worker(ILogger<Worker> logger, IOptions<Data> restrictedPhrases, AiBot bot, ILoggerFactory loggerFactory)
    : BackgroundService
{
    private readonly TwitchClient _client = new();

    private void SendMessage(string message, string username = "")
    {
        logger.LogInformation("Bot is sending message: {Message} in channel {Username}", message, username);

        _client.SendMessage("Bapes", message);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var credentials = new ConnectionCredentials("GolfClapBot", "token");

        _client.Initialize(credentials, "Bapes");

        _client.OnJoinedChannel += ClientOnOnJoinedChannel;
        _client.OnMessageReceived += ClientOnOnMessageReceived;
        _client.OnConnected += ClientOnOnConnected;

        await _client.ConnectAsync();

        logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
    }

    private Task ClientOnOnConnected(object? sender, OnConnectedArgs e)
    {
        logger?.LogInformation("Connected to channel [{Channel}]!", e.AutoJoinChannel);

        return Task.CompletedTask;
    }

    private async Task ClientOnOnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.DisplayName == "GolfClapBot")
            return;

        if (e.ChatMessage.Message.Length == 0)
            return;

        if (string.IsNullOrEmpty(e.ChatMessage.Message))
            return;

        var response = await bot.AnalyzeChatMessage(RemoveEmotes(e.ChatMessage), e.ChatMessage.Username);

        if (restrictedPhrases.Value.RestrictedPhrases != null && restrictedPhrases.Value.RestrictedPhrases.Exists(
                restrictedPhrase =>
                    response.Contains(restrictedPhrase.ToLower(), StringComparison.InvariantCultureIgnoreCase)))
        {
            return;
        }

        SendMessage(response, e.ChatMessage.Username);
    }

    private Task ClientOnOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Console.WriteLine($"Connected to {e.Channel}");
        SendMessage(
            $"Hello! I'm GolfClapBot, the AI chat bot developed by Bapes. If you're interested in learning more about it, please feel free to message him! v{GetVersion()}");

        return Task.CompletedTask;
    }

    private static string? GetVersion()
    {
        return typeof(Worker).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
    }

    private static string RemoveEmotes(ChatMessage msg)
    {
        StringBuilder parsed = new(msg.Message.Length);
        StringInfo rawInfo = new(msg.Message);

        var startIndex = 0;
        foreach (var emote in msg.EmoteSet.Emotes.OrderBy(x => x.StartIndex))
        {
            parsed.Append(rawInfo.SubstringByTextElements(startIndex, emote.StartIndex - startIndex));
            parsed.Replace("  ", " ");

            startIndex = emote.EndIndex + 1;
        }

        if (startIndex >= rawInfo.LengthInTextElements)
            return parsed.ToString();

        parsed.Append(rawInfo.SubstringByTextElements(startIndex));
        parsed.Replace("  ", " ");

        return parsed.ToString();
    }
}