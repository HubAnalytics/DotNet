namespace MicroserviceAnalytics.Core
{
    public interface IUserIdProvider
    {
        string UserId(IClientConfiguration configuration, object context);
    }
}
