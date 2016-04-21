namespace HubAnalytics.Core.Helpers
{
    public class PlatformDefaultConfigurationProvider : IClientConfigurationProvider
    {
        public IClientConfiguration Get()
        {
            #if DNXCORE50
                return new AspNet5Configuration();
            #else
                return HubAnalyticsConfigurationSection.Settings;
            #endif
        }
    }
}