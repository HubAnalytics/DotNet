using System;

namespace MicroserviceAnalytics.Core
{
    public abstract class DefaultClientConfiguration : IClientConfiguration
    {
        public abstract string PropertyId { get; }
        public abstract string Key { get; }
        public virtual TimeSpan UploadInterval => TimeSpan.FromMilliseconds(Constants.UploadIntervalMs);
        public virtual string ApiRoot => Constants.ApiRoot;
        public virtual string CorrelationIdKey => Constants.CorrelationIdKey;
        public virtual string SessionIdKey => Constants.SessionIdKey;
        public virtual string UserIdKey => Constants.UserIdKey;
        public bool EnableCorrelation => true;
        public bool StripHttpQueryParams => true;
        public string[] HttpRequestHeaderWhitelist => new[] {"*"};
        public string[] HttpResponseHeaderWhitelist => new[] {"*"};
        public string[] ExcludedVerbs => new[] {"OPTIONS"};
        public string HttpStopwatchKey => Constants.HttpStopwatchKey;
        public bool IsRemoteUpdateEnabled => false;
        public bool IsCaptureSqlEnabled => true;
        public bool IsCaptureHttpEnabled => true;
        public bool IsCaptureCustomMetricEnabled => true;
        public bool IsCaptureErrorsEnabled => true;
        public bool IsCaptureLogsEnabled => true;
        public bool IsUserTrackingEnabled => true;
        public bool IsSessionTrackingEnabled => true;
        public bool IsUserIdCreationEnabled => true;
        public bool IsSessionIdCreationEnabled => true;
        public string TrackingSessionCookieName => Constants.TrackingSessionCookieName;
        public string TrackingUserCookieName => Constants.TrackingUserCookieName;
    }
}
