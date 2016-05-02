using HubAnalytics.Core.HttpEventUrlParsers;
using Xunit;

namespace HubAnalytics.Core.Tests.HttpEventUrlParsers
{
    public class AzureQueueStorageParserTests
    {
        [Theory]
        [InlineData(@"https://afanalyticsdev.queue.core.windows.net:443/demoqueue/messages/22f2f789-5d1f-42e5-bba2-7666d40a9d6e?popreceipt=AgAAAAMAAAAAAAAAnQX3AIej0QE%3D&visibilitytimeout=30")]
        [InlineData(@"https://afanalyticsdev.queue.core.windows.net:443/demoqueue")]
        [InlineData(@"http://afanalyticsdev.queue.core.windows.net/demoqueue")]
        public void GetsDomainQueueNameType(string url)
        {
            // Arrange
            AzureQueueStorageParser parser = new AzureQueueStorageParser();
            string domain, name, type;

            // Act
            bool result = parser.Parse(url, out domain, out name, out type);

            // Assert
            Assert.Equal("afanalyticsdev.queue.core.windows.net", domain);
            Assert.Equal("demoqueue", name);
            Assert.Equal("azurequeue", type);
            Assert.True(result);
        }

        [Theory]
        [InlineData(@"https://afanalyticsdev.table.core.windows.net:443/sometable(PartitionKey='00000000000000000001',RowKey='2016-05-01')")]
        [InlineData(@"https://afanalyticsdev.blob.core.windows.net:443/something")]
        [InlineData(@"http://localhost")]
        public void FailsOnWrongPattern(string url)
        {
            // Arrange
            AzureQueueStorageParser parser = new AzureQueueStorageParser();
            string domain, name, type;

            // Act
            bool result = parser.Parse(url, out domain, out name, out type);

            // Assert
            Assert.Null(domain);
            Assert.Null(name);
            Assert.Null(type);
            Assert.False(result);
        }
    }
}
