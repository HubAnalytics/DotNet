using System;
using MicroserviceAnalytics.Core;
using Moq;
using Xunit;

namespace MicroserviceAnalytics.TraceListener.Tests
{
    public class LoggingTraceListenerTests
    {
        [Fact]
        public void GenericMessageLoggedAsInformation()
        {
            // Arrange
            Mock<IMicroserviceAnalyticClient> client = new Mock<IMicroserviceAnalyticClient>();
            Mock<IMicroserviceAnalyticClientFactory> factory = new Mock<IMicroserviceAnalyticClientFactory>();
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
            Mock<IMicroserviceAnalyticClient> client = new Mock<IMicroserviceAnalyticClient>();
            Mock<IMicroserviceAnalyticClientFactory> factory = new Mock<IMicroserviceAnalyticClientFactory>();
            factory.Setup(x => x.GetClient()).Returns(client.Object);
            LoggingTraceListener listener = new LoggingTraceListener(factory.Object);

            // Act
            listener.Write($"{level}: Hello world");

            // Assert
            client.Verify(x => x.Log("Hello world", (int)level, level.ToString(), It.IsAny<DateTimeOffset>(), null, null));
        }
    }
}
