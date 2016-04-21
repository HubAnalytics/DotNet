namespace HubAnalytics.Core.Helpers
{
    public interface IRuntimeProviderDiscoveryService
    {
        IUserIdProvider UserIdProvider { get; }

        ISessionIdProvider SessionIdProvider { get; }
    }
}
