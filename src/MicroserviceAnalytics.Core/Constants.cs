namespace MicroserviceAnalytics.Core
{
    public static class Constants
    {
        public const string ApiRoot = "https://collection.microserviceanalytics.com/";
        public const string CorrelationIdKey = "correlation-id";
        public const string HttpStopwatchKey = "httpStopwatch";
        public const string UserIdKey = "msa-user-id";
        public const string SessionIdKey = "msa-session-id";
        public const int UploadIntervalMs = 3000;
        public const string TrackingSessionCookieName = "msasessiontracker";
        public const string TrackingUserCookieName = "msausertracker";
        public const string TailCorrelationCookieName = "msatailcorrelation";
    }
}
