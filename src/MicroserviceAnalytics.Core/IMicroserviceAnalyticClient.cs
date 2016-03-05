using System;
using System.Collections.Generic;
using MicroserviceAnalytics.Core.Model;
using Environment = MicroserviceAnalytics.Core.Model.Environment;

namespace MicroserviceAnalytics.Core
{
    public interface IMicroserviceAnalyticClient
    {
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
    }
}