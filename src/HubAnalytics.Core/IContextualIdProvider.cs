namespace HubAnalytics.Core
{
    public interface IContextualIdProvider
    {
        string CorrelationId { get; set; }
        string UserId { get; set; }
        string SessionId { get; set; }
    }
}
