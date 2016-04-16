using System;
using System.Data.SqlClient;

namespace HubAnalytics.AzureSqlDatabase.Implementation
{
    internal class DataReaderToTelemetryItemMapper : IDataReaderToTelemetryItemMapper
    {
        public TelemetryItem Map(SqlDataReader reader)
        {
            TelemetryItem usage = new TelemetryItem
            {
                AvgCpuPercent = (double)(decimal)reader["AvgCpuPercent"],
                AvgDataIoPercent = (double)(decimal)reader["AvgDataIoPercent"],
                AvgDtuLimit = (int)reader["AvgDtuLimit"],
                AvgLogWritePercent = (double)(decimal)reader["AvgLogWritePercent"],
                AvgMaxSessionPercent = (double)(decimal)reader["AvgMaxSessionPercent"],
                AvgMaxWorkerPercent = (double)(decimal)reader["AvgMaxWorkerPercent"],
                AvgMemoryUsagePercent = (double)(decimal)reader["AvgMemoryUsagePercent"],
                AvgXtpStoragePercent = (double)(decimal)reader["AvgXtpStoragePercent"],
                MaxCpuPercent = (double)(decimal)reader["MaxCpuPercent"],
                MaxDataIoPercent = (double)(decimal)reader["MaxDataIoPercent"],
                MaxDtuLimit = (int)reader["MaxDtuLimit"],
                MaxLogWritePercent = (double)(decimal)reader["MaxLogWritePercent"],
                MaxMaxSessionPercent = (double)(decimal)reader["MaxMaxSessionPercent"],
                MaxMaxWorkerPercent = (double)(decimal)reader["MaxMaxWorkerPercent"],
                MaxMemoryUsagePercent = (double)(decimal)reader["MaxMemoryUsagePercent"],
                MaxXtpStoragePercent = (double)(decimal)reader["MaxXtpStoragePercent"],
                MinCpuPercent = (double)(decimal)reader["MinCpuPercent"],
                MinDataIoPercent = (double)(decimal)reader["MinDataIoPercent"],
                MinDtuLimit = (int)reader["MinDtuLimit"],
                MinLogWritePercent = (double)(decimal)reader["MinLogWritePercent"],
                MinMaxSessionPercent = (double)(decimal)reader["MinMaxSessionPercent"],
                MinMaxWorkerPercent = (double)(decimal)reader["MinMaxWorkerPercent"],
                MinMemoryUsagePercent = (double)(decimal)reader["MinMemoryUsagePercent"],
                MinXtpStoragePercent = (double)(decimal)reader["MinXtpStoragePercent"],
                PeriodBegin = new DateTimeOffset((DateTime)reader["PeriodBegin"], TimeSpan.FromHours(0))
            };
            return usage;
        }
    }
}
