using HubAnalytics.Core.Model;

namespace HubAnalytics.AzureSqlDatabase.Implementation
{
    internal interface ITelemetryItemToEventMapper
    {
        Event Map(TelemetryItem from);
    }
}
