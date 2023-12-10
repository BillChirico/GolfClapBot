using System.Reflection;
using System.Text;
using GolfClapBot.Bot;
using GolfClapBot.Domain.Configuration;
using Microsoft.Extensions.Options;
using TwitchLib.Api;
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
    private readonly IBot _bot;
    private readonly IOptions<Settings> _settings;
    private readonly Data _data;

    public TwitchWorker(ILogger<TwitchWorker> logger, ILoggerFactory loggerFactory, IBot bot, IOptions<Data> data,
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
        try
        {
            _logger.LogInformation("Bot received message: {Message}", e.ChatMessage.Message);

            if (e.ChatMessage.DisplayName == "GolfClapBot")
                return;

            if (string.IsNullOrEmpty(e.ChatMessage.Message))
                return;

            var m = RemoveEmotes(e.ChatMessage);

            if (string.IsNullOrEmpty(m.Trim()))
                return;

            if (_bot.RepliedMessages.Contains(m))
            {
                _logger.LogInformation("Bot has already replied to message: {Message}", m);

                await DeleteMessage(e.ChatMessage);

                return;
            }

            var response = await _bot.AnalyzeChatMessage(m, e.ChatMessage.Username);

            if (_data.RestrictedPhrases != null && _data.RestrictedPhrases.Exists(
                    restrictedPhrase =>
                        response.Contains(restrictedPhrase.ToLower(), StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            await SendMessage(response, e.ChatMessage.Username);
        }
        catch (Exception exception)
        {
            _logger.LogError("Error processing message: {Error}", exception.Message);
        }
    }

    private async Task<Task> TwitchClientOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        return SendMessage(await _bot.GetWelcomeMessage(GetVersion()));
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
        _logger.LogInformation("Bot is sending message: {Message} User: [{Username}]", message, username);

        return _client.SendMessageAsync("Bapes", message);
    }

    private static string? GetVersion()
    {
        return typeof(TwitchWorker).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
    }

    public static string RemoveEmotes(ChatMessage message)
    {
        StringBuilder parsed = new(message.Message);

        foreach (var emote in message.EmoteSet.Emotes.OrderByDescending(x => x.StartIndex))
        {
            parsed.Remove(emote.StartIndex, emote.EndIndex - emote.StartIndex + 1);
            parsed.Replace("  ", " ");
        }

        return parsed.ToString();
    }

    private async Task DeleteMessage(ChatMessage message)
    {
        try
        {
            _logger.LogWarning("Bot is deleting message: {MessageId}", message.Id);

            var twitchApi = new TwitchAPI
            {
                Settings =
                {
                    ClientId = _settings.Value.Twitch.ClientId, AccessToken = _settings.Value.Twitch.OAuthToken
                }
            };

            await twitchApi.Helix.Moderation.DeleteChatMessagesAsync(message.RoomId, "425816290", message.Id,
                _settings.Value.Twitch.OAuthToken);
        }
        catch (Exception exception)
        {
            _logger.LogError("Error deleting message: {Error}", exception.Message);
        }
    }

    protected override Task ExecuteAsync(CancellationToken stnoppingToken)
    {
        return Task.CompletedTask;
    }
}