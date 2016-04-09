using System;

namespace HubAnalytics.Core
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

        string[] ExcludedVerbs { get; }

        string HttpStopwatchKey { get; }

        bool IsRemoteUpdateEnabled { get; }

        string SessionIdKey { get; }

        string UserIdKey { get; }
        
        bool IsCaptureErrorsEnabled { get; }

        bool IsCaptureSqlEnabled { get; }

        bool IsCaptureHttpEnabled { get; }

        bool IsCaptureCustomMetricEnabled { get; }
        bool IsCaptureLogsEnabled { get; }
        bool IsUserTrackingEnabled { get; }
        bool IsSessionTrackingEnabled { get; }
        bool IsUserIdCreationEnabled { get; }
        bool IsSessionIdCreationEnabled { get; }
        string TrackingSessionCookieName { get; }
        string TrackingUserCookieName { get; }
        string TailCorrelationCookieName { get; }
    }
}
