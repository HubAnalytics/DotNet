using MicroserviceAnalytics.Core;
using MicroserviceAnalytics.Owin;
using Owin;

namespace MicroserviceAnalytics.OWIN
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
