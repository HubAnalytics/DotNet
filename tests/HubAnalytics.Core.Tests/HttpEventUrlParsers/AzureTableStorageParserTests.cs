using HubAnalytics.Core.HttpEventUrlParsers;
using Xunit;

namespace HubAnalytics.Core.Tests.HttpEventUrlParsers
{
    public class AzureTableStorageParserTests
    {
        [Theory]
        [InlineData(@"https://testaccount.table.core.windows.net:443/demotable(PartitionKey='00000000000000000001',RowKey='2016-05-01')")]
        [InlineData(@"https://testaccount.table.core.windows.net:443/demotable")]
        [InlineData(@"http://testaccount.table.core.windows.net/demotable/")]
        public void GetsDomainTableNameType(string url)
        {
            // Arrange
            AzureTableStorageParser parser = new AzureTableStorageParser();
            string domain, name, type;

            // Act
            bool result = parser.Parse(url, out domain, out name, out type);

            // Assert
            Assert.Equal("testaccount.table.core.windows.net", domain);
            Assert.Equal("demotable", name);
            Assert.Equal("azuretable", type);
            Assert.True(result);
        }

        [Theory]
        [InlineData(@"https://afanalyticsdev.blob.core.windows.net:443/sometable(PartitionKey='00000000000000000001',RowKey='2016-05-01')")]
        [InlineData(@"https://afanalyticsdev.queue.core.windows.net:443/something")]
        [InlineData(@"http://localhost")]
        public void FailsOnWrongPattern(string url)
        {
            // Arrange
            AzureTableStorageParser parser = new AzureTableStorageParser();
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
