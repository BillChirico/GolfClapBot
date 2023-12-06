using System.Reflection;
using System.Text;
using GolfClapBot.Runner.Configuration;
using Microsoft.Extensions.Options;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using ChatMessage = TwitchLib.Client.Models.ChatMessage;
using OnConnectedEventArgs = TwitchLib.Client.Events.OnConnectedEventArgs;

namespace GolfClapBot.Runner;

public class TwitchWorker : BackgroundService
{
    private readonly TwitchClient _client;
    private readonly ILogger<TwitchWorker> _logger;
    private readonly AiBot _bot;
    private readonly IOptions<Settings> _settings;
    private readonly Data _data;

    public TwitchWorker(ILogger<TwitchWorker> logger, ILoggerFactory loggerFactory, AiBot bot, IOptions<Data> data,
        IOptions<Settings> settings)
    {
        _logger = logger;
        _bot = bot;
        _settings = settings;
        _data = data.Value;
        _client = new TwitchClient(loggerFactory: loggerFactory);

        _client.OnConnected += TwitchClientOnConnected;
        _client.OnDisconnected += TwitchClientOnDisconnected;
        _client.OnMessageReceived += TwitchClientOnMessageReceived;
        _client.OnJoinedChannel += TwitchClientOnJoinedChannel;

        var credentials = new ConnectionCredentials(_settings.Value.Twitch.BotUser, _settings.Value.Twitch.OAuthToken);

        _client.Initialize(credentials, _settings.Value.Twitch.Channel);
        _client.ConnectAsync();
    }

    private async Task TwitchClientOnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.DisplayName == "GolfClapBot")
            return;

        if (string.IsNullOrEmpty(e.ChatMessage.Message))
            return;

        var b = RemoveEmotes(e.ChatMessage);

        var response = await _bot.AnalyzeChatMessage(e.ChatMessage.Message, e.ChatMessage.Username);

        if (_data.RestrictedPhrases != null && _data.RestrictedPhrases.Exists(
                restrictedPhrase =>
                    response.Contains(restrictedPhrase.ToLower(), StringComparison.InvariantCultureIgnoreCase)))
        {
            return;
        }

        await SendMessage(response, e.ChatMessage.Username);
    }

    private Task TwitchClientOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Console.WriteLine($"Connected to {e.Channel}");
        return SendMessage(
            $"Hello! I'm GolfClapBot, the AI chat bot developed by Bapes. If you're interested in learning more about it, please feel free to message him! v{GetVersion()}");
    }

    private Task TwitchClientOnDisconnected(object? sender, OnDisconnectedEventArgs e)
    {
        _logger.LogWarning("Worker disconnected from Twitch");

        return Task.CompletedTask;
    }

    private Task TwitchClientOnConnected(object? sender, OnConnectedEventArgs e)
    {
        _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

        return Task.CompletedTask;
    }

    private Task SendMessage(string message, string username = "")
    {
        _logger.LogInformation("Bot is sending message: {Message} in channel {Username}", message, username);

        return _client.SendMessageAsync("Bapes", message);
    }

    private static string? GetVersion()
    {
        return typeof(TwitchWorker).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
    }

    private static string RemoveEmotes(ChatMessage message)
    {
        StringBuilder parsed = new(message.Message);

        foreach (var emote in message.EmoteSet.Emotes.OrderByDescending(x => x.StartIndex))
        {
            parsed.Remove(emote.StartIndex, emote.EndIndex - emote.StartIndex + 1);
            parsed.Replace("  ", " ");
        }

        return parsed.ToString();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}