using GolfClapBot.Domain.Configuration;
using Microsoft.Extensions.Options;

namespace GolfClapBot.Test;

public class BotTests
{
    [Fact]
    public void Constructor_InitializesWithValidSettings_ShouldSetProperties()
    {
        // Arrange
        var mockData = new Mock<IOptions<Data>>();
        var mockSettings = new Mock<IOptions<Settings>>();
        var expectedData = new Data
        {
            TrainingData = null, RestrictedPhrases = null
        };
        var expectedSettings = new Settings
        {
            OpenAiSettings = new OpenAiSettings
            {
                ApiKey = "test-key", Model = null
            },
            TwitchSettings = null
        };
        mockData.Setup(m => m.Value).Returns(expectedData);
        mockSettings.Setup(m => m.Value).Returns(expectedSettings);

        // Act
        var bot = new Bot.Bot(mockData.Object, mockSettings.Object);

        // Assert
        Assert.NotNull(bot);
        Assert.NotNull(bot.AnalyzeChatMessage("test", "test"));
    }
}