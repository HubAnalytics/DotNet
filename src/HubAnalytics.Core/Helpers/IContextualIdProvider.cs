namespace HubAnalytics.Core.Helpers
{
    public interface IContextualIdProvider
    {
        string CorrelationId { get; set; }
        string UserId { get; set; }
        string SessionId { get; set; }
    }
}
