using System;

namespace MicroserviceAnalytics.Core
{
    public interface IClientConfiguration
    {
        string PropertyId { get; }

        string Key { get; }

        TimeSpan UploadInterval { get; }

        string ApiRoot { get; }

        string CorrelationIdKey { get; }

        bool EnableCorrelation { get; }
        
        bool StripHttpQueryParams { get; }

        string[] HttpRequestHeaderWhitelist { get; }

        string[] HttpResponseHeaderWhitelist { get; }

        string HttpStopwatchKey { get; }

        bool IsRemoteUpdateEnabled { get; }



        bool IsCaptureErrorsEnabled { get; }

        bool IsCaptureSqlEnabled { get; }

        bool IsCaptureHttpEnabled { get; }

        bool IsCaptureCustomMetricEnabled { get; }
        bool IsCaptureLogsEnabled { get; }
    }
}
