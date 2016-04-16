using System.Collections.Generic;
using System.Globalization;
using HubAnalytics.Core.Model;

namespace HubAnalytics.AzureSqlDatabase.Implementation
{
    internal class TelemetryItemToEventMapper : ITelemetryItemToEventMapper
    {
        public Event Map(TelemetryItem @from, string propertyId, string granularity, string connectionName, string connectionString)
        {
            return new Event
            {
                CorrelationDepths = null,
                CorrelationIds = null,
                Data = new Dictionary<string, object>
                {
                    { "AvgCpuPercent", @from.AvgCpuPercent },
                    { "MaxCpuPercent", @from.MaxCpuPercent  },
                    { "MinCpuPercent", @from.MinCpuPercent },
                    { "AvgDataIoPercent", @from.AvgDataIoPercent },
                    { "MaxDataIoPercent", @from.MaxDataIoPercent  },
                    { "MinDataIoPercent", @from.MinDataIoPercent },
                    { "AvgLogWritePercent", @from.AvgLogWritePercent },
                    { "MaxLogWritePercent", @from.MaxLogWritePercent  },
                    { "MinLogWritePercent", @from.MinLogWritePercent },
                    { "AvgMemoryUsagePercent", @from.AvgMemoryUsagePercent },
                    { "MaxMemoryUsagePercent", @from.MaxMemoryUsagePercent  },
                    { "MinMemoryUsagePercent", @from.MinMemoryUsagePercent },
                    { "AvgXtpStoragePercent", @from.AvgXtpStoragePercent },
                    { "MaxXtpStoragePercent", @from.MaxXtpStoragePercent  },
                    { "MinXtpStoragePercent", @from.MinXtpStoragePercent },
                    { "AvgMaxWorkerPercent", @from.AvgMaxWorkerPercent },
                    { "MaxMaxWorkerPercent", @from.MaxMaxWorkerPercent  },
                    { "MinMaxWorkerPercent", @from.MinMaxWorkerPercent },
                    { "AvgMaxSessionPercent", @from.AvgMaxSessionPercent },
                    { "MaxMaxSessionPercent", @from.MaxMaxSessionPercent  },
                    { "MinMaxSessionPercent", @from.MinMaxSessionPercent },
                    { "AvgDtuLimit", @from.AvgDtuLimit },
                    { "MaxDtuLimit", @from.MaxDtuLimit  },
                    { "MinDtuLimit", @from.MinDtuLimit },
                    { "Granularity", granularity },
                    { "ConnectionName", connectionName },
                    { "ConnectionString", connectionString },
                    { "PropertyId", propertyId }
                },
                EventEndDateTime = null,
                EventStartDateTime = @from.PeriodBegin.ToString(Event.EventDateFormat, CultureInfo.InvariantCulture),
                EventType = "azuresqldatabasetelemetry",
                SessionId = null,
                UserId = null
            };
        }
    }
}
