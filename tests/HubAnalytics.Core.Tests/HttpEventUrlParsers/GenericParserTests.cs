using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubAnalytics.Core.HttpEventUrlParsers;
using Xunit;

namespace HubAnalytics.Core.Tests.HttpEventUrlParsers
{
    public class GenericParserTests
    {
        [Fact]
        public void GetsSplitAndType()
        {
            // Arrange
            string url = @"https://localhost:1001/something/else";
            GenericParser parser = new GenericParser();
            string domain, name, type;

            // Act
            bool result = parser.Parse(url, out domain, out name, out type);

            // Assert
            Assert.Equal("localhost", domain);
            Assert.Equal("something", name);
            Assert.Equal("generic", type);
            Assert.True(result);
        }

        [Fact]
        public void GetsSplitNoSubPath()
        {
            // Arrange
            string url = @"https://localhost:1001/";
            GenericParser parser = new GenericParser();
            string domain, name, type;

            // Act
            bool result = parser.Parse(url, out domain, out name, out type);

            // Assert
            Assert.Equal("localhost", domain);
            Assert.Null(name);
            Assert.Equal("generic", type);
            Assert.True(result);
        }
    }
}
