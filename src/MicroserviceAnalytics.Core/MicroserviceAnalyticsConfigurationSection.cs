using System;
#if DNX451
using System.Configuration;

namespace MicroserviceAnalytics.Core
{
    class MicroserviceAnalyticsConfigurationSection : ConfigurationSection, IClientConfiguration
    {
        public static readonly MicroserviceAnalyticsConfigurationSection Settings = ConfigurationManager.GetSection("microserviceAnalyticSettings") as MicroserviceAnalyticsConfigurationSection ?? new MicroserviceAnalyticsConfigurationSection();

        [ConfigurationProperty("propertyId", IsRequired = true, DefaultValue = "")]
        [StringValidator]
        public string PropertyId
        {
            get { return (string)this["propertyId"]; }
            set { this["propertyId"] = value; }
        }

        [ConfigurationProperty("key", IsRequired = true, DefaultValue = "")]
        [StringValidator]
        public string Key
        {
            get { return (string)this["key"]; }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("uploadIntervalMs", IsRequired = false, DefaultValue = Constants.UploadIntervalMs)]
        public int UploadIntervalMs
        {
            get { return (int) this["uploadIntervalMs"]; }
            set { this["uploadIntervalMs"] = value; }
        }

        [ConfigurationProperty("apiRoot", IsRequired = false, DefaultValue = Constants.ApiRoot)]
        [StringValidator]
        public string ApiRoot {
            get { return (string) this["apiRoot"]; }
            set { this["apiRoot"] = value; }
        }

        [ConfigurationProperty("correlationIdKey", IsRequired = false, DefaultValue = Constants.CorrelationIdKey)]
        [StringValidator]
        public string CorrelationIdKey
        {
            get { return (string) this["correlationIdKey"]; }
            set { this["correlationIdKey"] = value; }
        }

        [ConfigurationProperty("enableCorrelation", IsRequired = false, DefaultValue = true)]
        public bool EnableCorrelation
        {
            get { return (bool)this["enableCorrelation"]; }
            set { this["enableCorrelation"] = value; }
        }

        [ConfigurationProperty("captureHttp", IsRequired = false, DefaultValue = true)]
        public bool IsCaptureHttpEnabled
        {
            get { return (bool)this["captureHttp"]; }
            set { this["captureHttp"] = value; }
        }

        [ConfigurationProperty("captureSql", IsRequired = false, DefaultValue = true)]
        public bool IsCaptureSqlEnabled
        {
            get { return (bool)this["captureSql"]; }
            set { this["captureSql"] = value; }
        }

        [ConfigurationProperty("captureErrors", IsRequired = false, DefaultValue = true)]
        public bool IsCaptureErrorsEnabled
        {
            get { return (bool)this["captureErrors"]; }
            set { this["captureErrors"] = value; }
        }

        [ConfigurationProperty("captureCustomMetrics", IsRequired = false, DefaultValue = true)]
        public bool IsCaptureCustomMetricEnabled
        {
            get { return (bool) this["captureCustomMetrics"]; }
            set { this["captureCustomMetrics"] = value; }
        }

        [ConfigurationProperty("captureLogs", IsRequired = false, DefaultValue = true)]
        public bool IsCaptureLogsEnabled
        {
            get { return (bool) this["captureLogs"]; }
            set { this["captureLogs"] = value; }
        }

        [ConfigurationProperty("stripHttpQueryParams", IsRequired = false, DefaultValue = true)]
        public bool StripHttpQueryParams
        {
            get { return (bool)this["stripHttpQueryParams"]; }
            set { this["stripHttpQueryParams"] = value; }
        }

        [ConfigurationProperty("httpRequestHeaderWhitelist", IsRequired = false, DefaultValue = "")]
        [StringValidator]
        public string HttpCommaSeparatedRequestHeaderWhitelist
        {
            get { return (string)this["httpRequestHeaderWhitelist"]; }
            set { this["httpRequestHeaderWhitelist"] = value; }
        }

        [ConfigurationProperty("httpResponseHeaderWhitelist", IsRequired = false, DefaultValue = "")]
        [StringValidator]
        public string HttpCommaSeparatedResponseHeaderWhitelist
        {
            get { return (string)this["httpResponseHeaderWhitelist"]; }
            set { this["httpResponseHeaderWhitelist"] = value; }
        }

        [ConfigurationProperty("excludedVerbs", IsRequired = false, DefaultValue = "OPTIONS")]
        [StringValidator]
        public string ExcludedVerbsCommaSeparated
        {
            get { return (string)this["excludedVerbs"]; }
            set { this["excludedVerbs"] = value; }
        }

        [ConfigurationProperty("httpStopwatchKey", IsRequired = false, DefaultValue = Constants.HttpStopwatchKey)]
        [StringValidator]
        public string HttpStopwatchKey
        {
            get { return (string)this["httpStopwatchKey"]; }
            set { this["httpStopwatchKey"] = value; }
        }

        [ConfigurationProperty("remoteUpdateEnabled", IsRequired = false, DefaultValue = false)]
        public bool IsRemoteUpdateEnabled
        {
            get { return (bool)this["remoteUpdateEnabled"]; }
            set { this["remoteUpdateEnabled"] = value; }
        }

        [ConfigurationProperty("userIdKey", IsRequired = false, DefaultValue = Constants.UserIdKey)]
        [StringValidator]
        public string UserIdKey
        {
            get { return (string)this["userIdKey"]; }
            set { this["userIdKey"] = value; }
        }

        [ConfigurationProperty("sessionIdKey", IsRequired = false, DefaultValue = Constants.SessionIdKey)]
        [StringValidator]
        public string SessionIdKey
        {
            get { return (string)this["sessionIdKey"]; }
            set { this["sessionIdKey"] = value; }
        }

        [ConfigurationProperty("isUserTrackingEnabled", IsRequired = false, DefaultValue = true)]
        public bool IsUserTrackingEnabled
        {
            get { return (bool) this["isUserTrackingEnabled"]; }
            set { this["isUserTrackingEnabled"] = value; }
        }

        [ConfigurationProperty("isSessionTrackingEnabled", IsRequired = false, DefaultValue = true)]
        public bool IsSessionTrackingEnabled
        {
            get { return (bool)this["isSessionTrackingEnabled"]; }
            set { this["isSessionTrackingEnabled"] = value; }
        }

        [ConfigurationProperty("isUserIdCreationEnabled", IsRequired = false, DefaultValue = true)]
        public bool IsUserIdCreationEnabled
        {
            get { return (bool)this["isUserIdCreationEnabled"]; }
            set { this["isUserIdCreationEnabled"] = value; }
        }

        [ConfigurationProperty("isSessionIdCreationEnabled", IsRequired = false, DefaultValue = true)]
        public bool IsSessionIdCreationEnabled
        {
            get { return (bool)this["isSessionIdCreationEnabled"]; }
            set { this["isSessionIdCreationEnabled"] = value; }
        }

        [ConfigurationProperty("trackingSessionCookieName", IsRequired = false, DefaultValue = Constants.TrackingSessionCookieName)]
        [StringValidator]
        public string TrackingSessionCookieName
        {
            get { return (string)this["trackingSessionCookieName"]; }
            set { this["trackingSessionCookieName"] = value; }
        }

        [ConfigurationProperty("trackingUserCookieName", IsRequired = false, DefaultValue = Constants.TrackingUserCookieName)]
        [StringValidator]
        public string TrackingUserCookieName
        {
            get { return (string)this["trackingUserCookieName"]; }
            set { this["trackingUserCookieName"] = value; }
        }

        [ConfigurationProperty("tailCorrelationCookieName", IsRequired = false, DefaultValue = Constants.TailCorrelationCookieName)]
        [StringValidator]
        public string TailCorrelationCookieName
        {
            get { return (string)this["tailCorrelationCookieName"]; }
            set { this["tailCorrelationCookieName"] = value; }
        }

        public string[] HttpRequestHeaderWhitelist => HttpCommaSeparatedRequestHeaderWhitelist.Split(',');
        public string[] HttpResponseHeaderWhitelist => HttpCommaSeparatedResponseHeaderWhitelist.Split(',');
        public string[] ExcludedVerbs => ExcludedVerbsCommaSeparated.Split(',');
        public TimeSpan UploadInterval => TimeSpan.FromMilliseconds(UploadIntervalMs);
    }
}
#endif