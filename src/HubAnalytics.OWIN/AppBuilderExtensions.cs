using HubAnalytics.Core;
using Owin;

namespace HubAnalytics.OWIN
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseHubAnalytics(this IAppBuilder appBuilder, IHubAnalyticsClientFactory hubAnalyticsClientFactory=null)
        {
            appBuilder.Use<CaptureMiddleware>(hubAnalyticsClientFactory);
            return appBuilder;
        }

    }
}
