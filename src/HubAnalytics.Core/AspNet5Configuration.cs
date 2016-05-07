using System;

namespace HubAnalytics.Core
{
#if DNXCORE50
    public class AspNet5Configuration : IClientConfiguration
    {
        public string PropertyId { get; }
        public string Key { get; }
        public TimeSpan UploadInterval { get; }
        public string ApiRoot { get; }
        public string CorrelationIdKey { get; }
        public bool EnableCorrelation { get; }
        public bool StripHttpQueryParams { get; }
        public string[] HttpRequestHeaderWhitelist { get; }
        public string[] HttpResponseHeaderWhitelist { get; }
        public string[] ExcludedVerbs { get; }
        public string HttpStopwatchKey { get; }
        public bool IsRemoteUpdateEnabled { get; }
        public bool IsCaptureErrorsEnabled { get; }
        public bool IsCaptureSqlEnabled { get; }
        public bool IsCaptureHttpEnabled { get; }
        public bool IsCaptureCustomMetricEnabled { get; }
        public bool IsCaptureLogsEnabled { get; }
        public bool IsCapturePageViewsEnabled { get; }
        public bool IsCaptureExternalHttpRequestsEnabled { get; }
        public bool IsUserTrackingEnabled { get; }
        public bool IsSessionTrackingEnabled { get; }
        public bool IsUserIdCreationEnabled { get; }
        public bool IsSessionIdCreationEnabled { get; }
        public string TrackingSessionCookieName { get; set; }
        public string TrackingUserCookieName { get; set; }
        public string TailCorrelationCookieName { get; set; }
        public string SessionIdKey { get; }
        public string UserIdKey { get; }
        public string ApplicationVersion { get; }
        public string ExtensionAssembly { get; }
    }
#endif
}
