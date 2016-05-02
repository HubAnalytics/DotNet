namespace HubAnalytics.Core.Model
{
    public static class EventTypes
    {
        public const string Error = "error";
        public const string Journey = "journey";
        public const string SqlCommand = "sqlcommand";
        public const string HttpTrace = "httptrace";
        public const string CustomMetric = "custommetric";
        public const string Log = "log";
        public const string ExternalHttpRequest = "externalhttprequest";
        public const string SimpleExternalHttpRequest = "simpleexternalhttprequest";
    }
}
