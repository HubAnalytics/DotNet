namespace MicroserviceAnalytics.Core
{
    public interface ISessionIdProvider
    {
        string SessionId(IClientConfiguration configuration, object context);
    }
}
