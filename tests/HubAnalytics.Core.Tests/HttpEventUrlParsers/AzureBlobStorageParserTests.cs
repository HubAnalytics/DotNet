using HubAnalytics.Core.HttpEventUrlParsers;
using Xunit;

namespace HubAnalytics.Core.Tests.HttpEventUrlParsers
{
    public class AzureBlobStorageParserTests
    {
        [Theory]
        [InlineData(@"https://testaccount.blob.core.windows.net:443/demoblobcontainer/16e9dcfd-cd94-43ba-a419-f22a6b574257.zas")]
        [InlineData(@"https://testaccount.blob.core.windows.net:443/demoblobcontainer")]
        [InlineData(@"http://testaccount.blob.core.windows.net/demoblobcontainer/")]
        public void GetsDomainBlobContainerNameType(string url)
        {
            // Arrange
            AzureBlobStorageParser parser = new AzureBlobStorageParser();
            string domain, name, type;

            // Act
            bool result = parser.Parse(url, out domain, out name, out type);

            // Assert
            Assert.Equal("testaccount.blob.core.windows.net", domain);
            Assert.Equal("demoblobcontainer", name);
            Assert.Equal("azureblob", type);
            Assert.True(result);
        }

        [Theory]
        [InlineData(@"https://afanalyticsdev.table.core.windows.net:443/sometable(PartitionKey='00000000000000000001',RowKey='2016-05-01')")]
        [InlineData(@"https://afanalyticsdev.queue.core.windows.net:443/something")]
        [InlineData(@"http://localhost")]
        public void FailsOnWrongPattern(string url)
        {
            // Arrange
            AzureBlobStorageParser parser = new AzureBlobStorageParser();
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
