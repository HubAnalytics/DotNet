namespace HubAnalytics.Core
{
    public interface ISessionIdProvider
    {
        string SessionId(IClientConfiguration configuration, object context);
    }
}
