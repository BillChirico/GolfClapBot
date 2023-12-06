using GolfClapBot.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GolfClapBot.Test;

public class StartupTests
{
    private readonly ServiceCollection _services = new();

    private readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            {
                "Data:SomeProperty", "Value1"
            },
            {
                "Settings:SomeProperty", "Value2"
            }
        }!)
        .Build();

    [Fact]
    public void ConfigureServices_RegistersLoggingServices_Correctly()
    {
        // Arrange
        var builder = new HostBuilder().ConfigureServices((context, services) =>
        {
            services.AddLogging();
            services.AddLogging(c => c.AddSimpleConsole());
        });

        // Act
        var host = builder.Build();
        var serviceProvider = host.Services;

        // Assert
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        Assert.NotNull(loggerFactory);
        Assert.IsType<LoggerFactory>(loggerFactory);
    }

    [Fact]
    public void ConfigureServices_BindsDataAndSettingsConfiguration_Correctly()
    {
        // Arrange
        _services.Configure<Data>(_configuration.GetSection(nameof(Data)));
        _services.Configure<Settings>(_configuration.GetSection(nameof(Settings)));

        // Act
        var serviceProvider = _services.BuildServiceProvider();
        var dataOptions = serviceProvider.GetService<IOptions<Data>>();
        var settingsOptions = serviceProvider.GetService<IOptions<Settings>>();

        // Assert
        Assert.NotNull(dataOptions);
        Assert.NotNull(settingsOptions);
    }
}