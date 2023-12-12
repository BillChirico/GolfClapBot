using GolfClapBot.Bot;
using GolfClapBot.Domain.Configuration;
using GolfClapBot.Runner;
using Serilog;
using TwitchLib.Api;
using TwitchLib.Api.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

// Services
builder.Services.AddHostedService<TwitchWorker>();
builder.Services.AddSingleton<IBot, Bot>();
builder.Services.AddSingleton<ITwitchAPI, TwitchAPI>(x => new TwitchAPI
{
    Settings =
    {
        AccessToken = builder.Configuration["Settings:TwitchSettings:OAuthToken"],
        ClientId = builder.Configuration["Settings:TwitchSettings:ClientId"]
    }
});

// Logging
// Need to clear the providers added by the HostBuilder
builder.Logging.ClearProviders();

builder.Logging.AddSerilog(dispose: true).AddConfiguration(builder.Configuration);

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog(dispose: true);
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

// Configuration
builder.Services.Configure<Data>(
    builder.Configuration.GetSection(nameof(Data)));
builder.Services.Configure<Settings>(
    builder.Configuration.GetSection(nameof(Settings)));

var host = builder.Build();
host.Run();