using System.Data.SqlClient;

namespace HubAnalytics.AzureSqlDatabase.Implementation
{
    internal interface IDataReaderToTelemetryItemMapper
    {
        TelemetryItem Map(SqlDataReader reader);
    }
}
