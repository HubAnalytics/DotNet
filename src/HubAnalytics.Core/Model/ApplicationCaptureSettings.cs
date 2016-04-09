namespace HubAnalytics.Core.Model
{
    public class ApplicationCaptureSettings
    {
        public string PropertyId { get; set; }

        public int UploadIntervalMs { get; set; }

        public bool IsCaptureHttpEnabled { get; set; }

        public bool IsCaptureErrorsEnabled { get; set; }

        public bool IsCaptureSqlEnabled { get; set; }

        public bool IsCaptureCustomMetricsEnabled { get; set; }

        public bool IsCaptureLogsEnabled { get; set; }

        public bool IsUserTrackingEnabled { get; set; }

        public bool IsSessionTrackingEnabled { get; set; }
    }
}
