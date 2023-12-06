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
            OpenAi = new OpenAI
            {
                ApiKey = "test-key", Model = null
            },
            Twitch = null
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