using System;
using HubAnalytics.Core.Helpers;
using HubAnalytics.Core.Model;
using SimpleJSON;

namespace HubAnalytics.Core.Implementation
{
    internal class DefaultJsonSerialization : IJsonSerialization
    {
        public T Deserialize<T>(string serializedJson) where T : class
        {
            // This is left genericised as we may swing back and implement fuller Json support and so hiding this detail from main code line
            if (typeof (T) != typeof (ApplicationCaptureSettings))
            {
                throw new InvalidCastException("T must be of type ApplicationCaptureSettings");                
            }
            JObject jObject = JSONDecoder.Decode(serializedJson);
            object applicationCaptureSettings = new ApplicationCaptureSettings
            {
                PropertyId = (string)jObject["PropertyId"],
                IsCaptureCustomMetricsEnabled = (bool)jObject["IsCaptureCustomMetricsEnabled"],
                IsCaptureErrorsEnabled = (bool)jObject["IsCaptureErrorsEnabled"],
                IsCaptureHttpEnabled = (bool)jObject["IsCaptureHttpEnabled"],
                IsCaptureLogsEnabled = (bool)jObject["IsCaptureLogsEnabled"],
                IsCaptureSqlEnabled = (bool)jObject["IsCaptureSqlEnabled"],
                IsSessionTrackingEnabled = (bool)jObject["IsSessionTrackingEnabled"],
                IsUserTrackingEnabled = (bool)jObject["IsUserTrackingEnabled"],
                IsCapturePageViewEnabled = (bool)jObject["IsCapturePageViewEnabled"],
                IsCaptureExternalHttpRequestsEnabled = (bool)jObject["IsCaptureExternalHttpRequestsEnabled"],
                UploadIntervalMs = (int)jObject["UploadIntervalMs"]
            };
            return (T) applicationCaptureSettings;
        }

        public object Deserialize(string serialziedJson)
        {
            return JSONDecoder.Decode(serialziedJson);
        }

        public string Serialize(object val)
        {
            return JSONEncoder.Encode(val);
        }
    }
}
