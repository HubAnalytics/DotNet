namespace MicroserviceAnalytics.Core
{
    public interface IMicroserviceAnalyticClientFactory
    {
        IMicroserviceAnalyticClient GetClient();
        IClientConfiguration GetClientConfiguration();
        IContextualIdProvider GetCorrelationIdProvider();
        IEnvironmentCapture GetEnvironmentCapture();
        IStackTraceParser GetStackTraceParser();
        IRuntimeProviderDiscoveryService GetRuntimeProviderDiscoveryService();
    }
}