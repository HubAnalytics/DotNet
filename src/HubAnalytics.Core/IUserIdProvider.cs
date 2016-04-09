namespace HubAnalytics.Core
{
    public interface IUserIdProvider
    {
        string UserId(IClientConfiguration configuration, object context);
    }
}
