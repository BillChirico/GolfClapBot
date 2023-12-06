using GolfClapBot.Bot;
using GolfClapBot.Domain.Configuration;
using GolfClapBot.Runner;

var builder = Host.CreateApplicationBuilder(args);

// Logging
builder.Services.AddLogging();
builder.Services.AddLogging(c => c.AddSimpleConsole());

// Services
builder.Services.AddHostedService<TwitchWorker>();
builder.Services.AddSingleton<IBot, Bot>();

// Configuration
builder.Services.Configure<Data>(
    builder.Configuration.GetSection(nameof(Data)));
builder.Services.Configure<Settings>(
    builder.Configuration.GetSection(nameof(Settings)));

var host = builder.Build();
host.Run();