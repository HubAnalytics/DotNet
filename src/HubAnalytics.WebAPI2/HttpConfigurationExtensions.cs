using System.Web.Http;
using HubAnalytics.Core;

namespace HubAnalytics.WebAPI2
{
    public static class HttpConfigurationExtensions
    {
        public static HttpConfiguration EnableErrorCapture(this HttpConfiguration configuration, IHubAnalyticsClientFactory hubAnalyticsClientFactory = null)
        {
            configuration.Filters.Add(new ExceptionCaptureFilter(hubAnalyticsClientFactory));
            return configuration;
        }

        public static HttpConfiguration EnableCorrelation(this HttpConfiguration configuration, IHubAnalyticsClientFactory hubAnalyticsClientFactory = null)
        {
            configuration.Filters.Add(new HttpCorrelatorAttribute(hubAnalyticsClientFactory));
            return configuration;
        }
    }
}
