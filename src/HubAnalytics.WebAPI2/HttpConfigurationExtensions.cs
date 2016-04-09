using System.Web.Http;
using HubAnalytics.Core;

namespace HubAnalytics.WebAPI2
{
    public static class HttpConfigurationExtensions
    {
        public static HttpConfiguration EnableErrorCapture(this HttpConfiguration configuration, MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory = null)
        {
            configuration.Filters.Add(new ExceptionCaptureFilter(microserviceAnalyticClientFactory));
            return configuration;
        }

        public static HttpConfiguration EnableCorrelation(this HttpConfiguration configuration, MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory = null)
        {
            configuration.Filters.Add(new HttpCorrelatorAttribute(microserviceAnalyticClientFactory));
            return configuration;
        }
    }
}
