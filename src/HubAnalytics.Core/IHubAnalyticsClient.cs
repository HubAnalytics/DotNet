using System;
using System.Collections.Generic;
using HubAnalytics.Core.Model;
using Environment = HubAnalytics.Core.Model.Environment;

namespace HubAnalytics.Core
{
    public interface IHubAnalyticsClient
    {
        void Stop();
        void RegisterTelemetryProvider(ITelemetryEventProvider telemetryEventProvider);

        IClientConfiguration ClientConfiguration { get; }

        IReadOnlyCollection<Event> GetEvents(int batchSize);

        Environment GetEnvironment();

        void SqlCommand(
            DateTimeOffset executedAt,
            string connectionString,
            string commandText,
            int durationInMilliseconds,
            bool succeeded,
            int? sqlErrorCode);

        void Error(Exception ex, Dictionary<string, string> additionalData = null, string correlationId = null, string sessionId = null, string userId = null);

        void HttpRequest(string verb,
            int statusCode,
            string uri,
            bool didStripQueryParams,
            string correlationId,
            string sessionId,
            string userId,
            DateTimeOffset requestDateTime,
            long durationInMilliseconds,
            Dictionary<string, string[]> requestHeaders,
            Dictionary<string, string[]> responseHeaders);

        void Indicator(string indicatorType, double value);

        void Log(string message,
            int levelRank,
            string levelText,
            DateTimeOffset timestamp,
            Exception ex,
            object payload);

        void LogWithJson(string message,
            int levelRank,
            string levelText,
            DateTimeOffset timestamp,
            Exception exception, string payloadJsonString);

        void ExternalHttpRequest(
            DateTimeOffset requestedAt,
            long durationInMilliseconds,
            bool? success,
            string name,
            string domain,
            string type);
    }
}