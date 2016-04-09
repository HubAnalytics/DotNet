namespace HubAnalytics.Core
{
    public interface IHubAnalyticsClientFactory
    {
        IHubAnalyticsClient GetClient();
        IClientConfiguration GetClientConfiguration();
        IContextualIdProvider GetCorrelationIdProvider();
        IEnvironmentCapture GetEnvironmentCapture();
        IStackTraceParser GetStackTraceParser();
        IRuntimeProviderDiscoveryService GetRuntimeProviderDiscoveryService();
    }
}