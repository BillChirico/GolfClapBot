using Bapes.Chatbot.Worker;
using Bapes.ChatBot.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Services
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<AiBot>();

// Configuration
builder.Services.Configure<Data>(
    builder.Configuration.GetSection(nameof(Data)));
builder.Services.Configure<OpenApiKey>(
    builder.Configuration.GetSection("OpenAi"));

var host = builder.Build();
host.Run();