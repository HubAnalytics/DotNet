namespace MicroserviceAnalytics.Core
{
    public interface IMicroserviceAnalyticClientFactory
    {
        IMicroserviceAnalyticClient GetClient();
        IClientConfiguration GetClientConfiguration();
        ICorrelationIdProvider GetCorrelationIdProvider();
        IEnvironmentCapture GetEnvironmentCapture();
        IStackTraceParser GetStackTraceParser();
    }
}