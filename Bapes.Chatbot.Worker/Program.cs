using Bapes.Chatbot.Worker;
using Bapes.ChatBot.Worker;
using Bapes.ChatBot.Worker.Configuration;

var builder = Host.CreateApplicationBuilder(args);

// Logging
builder.Services.AddLogging();
builder.Services.AddLogging(c => c.AddSimpleConsole());

// Services
builder.Services.AddHostedService<TwitchWorker>();
builder.Services.AddSingleton<AiBot>();

// Configuration
builder.Services.Configure<Data>(
    builder.Configuration.GetSection(nameof(Data)));
builder.Services.Configure<Settings>(
    builder.Configuration.GetSection(nameof(Settings)));

var host = builder.Build();
host.Run();