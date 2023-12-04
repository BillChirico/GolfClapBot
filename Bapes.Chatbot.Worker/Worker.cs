using System.Globalization;
using System.Reflection;
using System.Text;
using Bapes.Chatbot.Worker;
using Microsoft.Extensions.Options;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Bapes.ChatBot.Worker;

public class Worker(ILogger<Worker> logger, IOptions<RestrictedPhrases> restrictedPhrases, AiBot bot)
    : BackgroundService
{
    private TwitchClient _client;

    private void Client_OnLog(object? sender, OnLogArgs e)
    {
        logger.LogInformation("{S}: {BotUsername} - {Data}", e.DateTime.ToString(CultureInfo.InvariantCulture),
            e.BotUsername, e.Data);
    }

    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyVersion = assembly.GetName().Version;

        Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        SendMessage(
            $"Hello! I'm GolfClapBot, the AI chat bot developed by Bapes. If you're interested in learning more about it, please feel free to message him! v{GetVersion()}");
    }

    private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        logger.LogInformation("Connected to channel [{Channel}]!", e.Channel);
    }

    private async void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.DisplayName == "GolfClapBot")
            return;

        if (e.ChatMessage.Message.Length == 0)
            return;

        if (string.IsNullOrEmpty(e.ChatMessage.Message))
            return;

        var response = await bot.AnalyzeChatMessage(RemoveEmojis(e.ChatMessage), e.ChatMessage.Username);

        if (restrictedPhrases.Value.Phrases != null && restrictedPhrases.Value.Phrases.Exists(restrictedPhrase =>
                response.Contains(restrictedPhrase.ToLower(), StringComparison.InvariantCultureIgnoreCase)))
        {
            return;
        }

        SendMessage(response, e.ChatMessage.Username);
    }

    private void SendMessage(string message, string username = "")
    {
        logger.LogInformation("Bot is sending message: {Message} in channel {Username}", message, username);

        _client.SendMessage("Bapes", message);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var credentials = new ConnectionCredentials("GolfClapBot", "7x02rd4r3tnm0k7j2ayiewx1hrchkg");

                var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 750, ThrottlingPeriod = TimeSpan.FromSeconds(30)
                };

                var customClient = new WebSocketClient(clientOptions);
                _client = new TwitchClient();
                _client = new TwitchClient(customClient);
                _client.Initialize(credentials, "Bapes");

                _client.OnLog += Client_OnLog;
                _client.OnJoinedChannel += Client_OnJoinedChannel;
                _client.OnMessageReceived += Client_OnMessageReceived;
                _client.OnConnected += Client_OnConnected;

                _client.Connect();
                _client.Connect();

                logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

                Thread.Sleep(Timeout.Infinite);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private string? GetVersion()
    {
        return typeof(Worker).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
    }

    private string RemoveEmojis(ChatMessage message)
    {
        StringBuilder parsed = new(message.Message);

        foreach (var emote in message.EmoteSet.Emotes.OrderByDescending(x => x.StartIndex))
        {
            parsed.Remove(emote.StartIndex, emote.EndIndex - emote.StartIndex + 1);
            parsed.Replace("  ", " ");
        }

        return parsed.ToString();
    }
}