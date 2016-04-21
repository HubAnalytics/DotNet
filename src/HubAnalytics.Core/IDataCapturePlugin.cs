namespace HubAnalytics.Core
{
    public interface IDataCapturePlugin
    {
        void Initialize(IHubAnalyticsClient client);
    }
}
