using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace HubAnalytics.AzureSqlDatabase.Implementation
{
    internal class SqlByHourProvider : IUsageProvider
    {
        private readonly IDataReaderToTelemetryItemMapper _mapper;

        public SqlByHourProvider(IDataReaderToTelemetryItemMapper mapper)
        {
            _mapper = mapper;
        }

        // the where clause ensures we haven't lost information from the start of the hour unit as it falls out of the bottom
        // of the sys.dm_db_resource_stats table by ensuring we only log data for minutes where we can see the first item (i.e. their is a data item
        // still in the previous time period)
        private const string CommandText = @"
SELECT dateadd(ms,-datepart(ms,end_time),dateadd(second,-datepart(second,end_time), dateadd(minute,-datepart(minute,end_time),end_time))) as PeriodBegin,
       avg(avg_cpu_percent) as AvgCpuPercent,
	   min(avg_cpu_percent) as MinCpuPercent,
       max(avg_cpu_percent) as MaxCpuPercent,
	   avg(avg_data_io_percent) as AvgDataIoPercent,
	   min(avg_data_io_percent) as MinDataIoPercent,
       max(avg_data_io_percent) as MaxDataIoPercent,
	   avg(avg_log_write_percent) as AvgLogWritePercent,
	   min(avg_log_write_percent) as MinLogWritePercent,
       max(avg_log_write_percent) as MaxLogWritePercent,
       avg(avg_memory_usage_percent) as AvgMemoryUsagePercent,
	   min(avg_memory_usage_percent) as MinMemoryUsagePercent,
	   max(avg_memory_usage_percent) as MaxMemoryUsagePercent,
	   avg(xtp_storage_percent) as AvgXtpStoragePercent,
	   min(xtp_storage_percent) as MinXtpStoragePercent,
	   max(xtp_storage_percent) as MaxXtpStoragePercent,
	   avg(max_worker_percent) as AvgMaxWorkerPercent,
	   min(max_worker_percent) as MinMaxWorkerPercent,
	   max(max_worker_percent) as MaxMaxWorkerPercent,
	   avg(max_session_percent) as AvgMaxSessionPercent,
	   min(max_session_percent) as MinMaxSessionPercent,
	   max(max_session_percent) as MaxMaxSessionPercent,
	   avg(dtu_limit) as AvgDtuLimit,
	   min(dtu_limit) as MinDtuLimit,
	   max(dtu_limit) as MaxDtuLimit
  FROM sys.dm_db_resource_stats
 where (select min(end_time) from sys.dm_db_resource_stats) < dateadd(ms,-datepart(ms,end_time),dateadd(second,-datepart(second,end_time), dateadd(minute,-datepart(minute,end_time),end_time)))
 GROUP BY dateadd(ms,-datepart(ms,end_time),dateadd(second,-datepart(second,end_time), dateadd(minute,-datepart(minute,end_time),end_time)))
 order by PeriodBegin desc";

        public async Task<IReadOnlyCollection<TelemetryItem>> Get(AzureSqlDatabase azureSqlDatabase)
        {
            List<TelemetryItem> result = new List<TelemetryItem>();
            using (SqlConnection connection = new SqlConnection(azureSqlDatabase.ConnectionString))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand(CommandText, connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    TelemetryItem usage = _mapper.Map(reader);
                    result.Add(usage);
                }
            }
            return result;
        }

        public string Granularity => "hour";
    }
}
