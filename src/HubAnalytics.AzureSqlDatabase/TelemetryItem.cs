using System;

namespace HubAnalytics.AzureSqlDatabase
{
    public class TelemetryItem
    {
        public DateTimeOffset PeriodBegin { get; set; }

        public double AvgCpuPercent { get; set; }

        public double MaxCpuPercent { get; set; }

        public double MinCpuPercent { get; set; }

        public double AvgDataIoPercent { get; set; }

        public double MinDataIoPercent { get; set; }

        public double MaxDataIoPercent { get; set; }

        public double AvgLogWritePercent { get; set; }

        public double MinLogWritePercent { get; set; }

        public double MaxLogWritePercent { get; set; }

        public double AvgMemoryUsagePercent { get; set; }

        public double MinMemoryUsagePercent { get; set; }

        public double MaxMemoryUsagePercent { get; set; }

        public double AvgXtpStoragePercent { get; set; }

        public double MinXtpStoragePercent { get; set; }

        public double MaxXtpStoragePercent { get; set; }

        public double AvgMaxWorkerPercent { get; set; }

        public double MinMaxWorkerPercent { get; set; }

        public double MaxMaxWorkerPercent { get; set; }

        public double AvgMaxSessionPercent { get; set; }

        public double MinMaxSessionPercent { get; set; }

        public double MaxMaxSessionPercent { get; set; }

        public int AvgDtuLimit { get; set; }

        public int MinDtuLimit { get; set; }

        public int MaxDtuLimit { get; set; }
    }
}
