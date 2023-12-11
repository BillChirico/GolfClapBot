using System.Reflection;
using System.Text;
using GolfClapBot.Bot;
using GolfClapBot.Domain.Configuration;
using Microsoft.Extensions.Options;
using TwitchLib.Api.Interfaces;
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
    private readonly ITwitchAPI _twitchApi;
    private readonly Data _data;
    private readonly List<ChatMessage> _sentMessages = [];

    public TwitchWorker(ILogger<TwitchWorker> logger, ILoggerFactory loggerFactory, IBot bot, IOptions<Data> data,
        IOptions<Settings> settings, ITwitchAPI twitchApi)
    {
        _logger = logger;
        _bot = bot;
        _settings = settings;
        _twitchApi = twitchApi;
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

    /// <summary>
    ///     Event handler for the Twitch client's OnMessageReceived event.
    ///     Processes received chat messages and performs necessary actions.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An instance of the OnMessageReceivedArgs class containing the received message.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task TwitchClientOnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        try
        {
            _logger.LogInformation("Bot received message: {Message}", e.ChatMessage.Message);

            if (e.ChatMessage.DisplayName == "GolfClapBot")
            {
                _logger.LogInformation("Bot is ignoring message from itself");

                return;
            }

            // Remove emotes from message
            var m = RemoveEmotes(e.ChatMessage);

            if (string.IsNullOrEmpty(m.Trim()))
            {
                _logger.LogInformation("Bot is ignoring empty message: {Message}", m);

                return;
            }

            if (_sentMessages.Exists(message => message.TmiSent > DateTime.UtcNow.AddSeconds(-5)))
            {
                _logger.LogInformation("Bot has already sent a message in the last 5 seconds");

                return;
            }

            if (_bot.RepliedMessages.Contains(m) && _sentMessages.Find(message => message.Message == m)?.TmiSent >
                DateTime.UtcNow.AddMinutes(-5))
            {
                _logger.LogInformation("Bot has already replied to the same message within five minutes: {Message}", m);

                await DeleteMessage(e.ChatMessage);

                return;
            }

            var response = await _bot.AnalyzeChatMessage(m, e.ChatMessage.Username);

            if (_data.RestrictedPhrases != null && _data.RestrictedPhrases.Exists(
                    restrictedPhrase =>
                        response.Contains(restrictedPhrase.ToLower(), StringComparison.InvariantCultureIgnoreCase)))
            {
                _logger.LogInformation("Bot is deleting message: {Message}", m);

                return;
            }

            _sentMessages.Add(e.ChatMessage);

            await SendMessage(response, e.ChatMessage.Username);
        }
        catch (Exception exception)
        {
            _logger.LogError("Error processing message: {Error}", exception.Message);
        }
    }

    private async Task<Task> TwitchClientOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        _logger.LogInformation("Bot joined channel: {Channel}", e.Channel);

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

    /// <summary>
    ///     Retrieves the version of the TwitchWorker assembly.
    /// </summary>
    /// <typeparam name="T">The type of the assembly.</typeparam>
    /// <returns>
    ///     The version of the TwitchWorker assembly, represented as a string, or null if the version information is not
    ///     available.
    /// </returns>
    private static string? GetVersion()
    {
        return typeof(TwitchWorker).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
    }

    /// <summary>
    ///     Removes emotes from the given chat message.
    /// </summary>
    /// <param name="message">The chat message containing the emotes.</param>
    /// <returns>The chat message with emotes removed.</returns>
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

    /// <summary>
    ///     Deletes a chat message.
    /// </summary>
    /// <param name="message">The chat message to be deleted.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task DeleteMessage(ChatMessage message)
    {
        try
        {
            _logger.LogWarning("Bot is deleting message: {MessageId}", message.Id);

            await _twitchApi.Helix.Moderation.DeleteChatMessagesAsync(message.RoomId, "425816290", message.Id,
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