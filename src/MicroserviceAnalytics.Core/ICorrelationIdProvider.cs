namespace MicroserviceAnalytics.Core
{
    public interface ICorrelationIdProvider
    {
        string CorrelationId { get; set; }
    }
}
