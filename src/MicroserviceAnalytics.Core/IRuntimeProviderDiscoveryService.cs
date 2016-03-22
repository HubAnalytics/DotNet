namespace MicroserviceAnalytics.Core
{
    public interface IRuntimeProviderDiscoveryService
    {
        IUserIdProvider UserIdProvider { get; }

        ISessionIdProvider SessionIdProvider { get; }
    }
}
