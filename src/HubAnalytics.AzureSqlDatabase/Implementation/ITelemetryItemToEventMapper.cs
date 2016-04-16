using HubAnalytics.Core.Model;

namespace HubAnalytics.AzureSqlDatabase.Implementation
{
    internal interface ITelemetryItemToEventMapper
    {
        Event Map(TelemetryItem from, string propertyId, string granularity, string connectionName, string connectionString);
    }
}
