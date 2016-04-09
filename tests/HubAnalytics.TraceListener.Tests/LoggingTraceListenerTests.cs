using System;
using HubAnalytics.Core;
using Moq;
using Xunit;

namespace HubAnalytics.TraceListener.Tests
{
    public class LoggingTraceListenerTests
    {
        [Fact]
        public void GenericMessageLoggedAsInformation()
        {
            // Arrange
            Mock<IHubAnalyticsClient> client = new Mock<IHubAnalyticsClient>();
            Mock<IHubAnalyticsClientFactory> factory = new Mock<IHubAnalyticsClientFactory>();
            factory.Setup(x => x.GetClient()).Returns(client.Object);
            LoggingTraceListener listener = new LoggingTraceListener(factory.Object);

            // Act
            listener.Write("Hello world");

            // Assert
            client.Verify(x => x.Log("Hello world", (int)LogEventLevelEnum.Information, "Information", It.IsAny<DateTimeOffset>(), null, null));
        }

        [Theory]
        [InlineData(LogEventLevelEnum.Information)]
        [InlineData(LogEventLevelEnum.Debug)]
        [InlineData(LogEventLevelEnum.Warning)]
        [InlineData(LogEventLevelEnum.Error)]
        [InlineData(LogEventLevelEnum.Fatal)]
        [InlineData(LogEventLevelEnum.Verbose)]
        public void GenericMessageLoggedAsInformation(LogEventLevelEnum level)
        {
            // Arrange
            Mock<IHubAnalyticsClient> client = new Mock<IHubAnalyticsClient>();
            Mock<IHubAnalyticsClientFactory> factory = new Mock<IHubAnalyticsClientFactory>();
            factory.Setup(x => x.GetClient()).Returns(client.Object);
            LoggingTraceListener listener = new LoggingTraceListener(factory.Object);

            // Act
            listener.Write($"{level}: Hello world");

            // Assert
            client.Verify(x => x.Log("Hello world", (int)level, level.ToString(), It.IsAny<DateTimeOffset>(), null, null));
        }
    }
}
