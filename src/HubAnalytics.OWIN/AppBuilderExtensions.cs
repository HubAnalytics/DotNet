using HubAnalytics.Core;
using Owin;

namespace HubAnalytics.OWIN
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseMicroserviceAnalytics(this IAppBuilder appBuilder, MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory=null)
        {
            appBuilder.Use<CaptureMiddleware>(microserviceAnalyticClientFactory);
            return appBuilder;
        }

    }
}
