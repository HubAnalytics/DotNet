using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MicroserviceAnalytics.Core.Model;
using Newtonsoft.Json;
using Environment = MicroserviceAnalytics.Core.Model.Environment;

namespace MicroserviceAnalytics.Core.Implementation
{
    internal class MicroserviceAnalyticClient : IMicroserviceAnalyticClient
    {
        private readonly ConcurrentQueue<Event> _eventQueue = new ConcurrentQueue<Event>();
        private readonly IEnvironmentCapture _environmentCapture;
        private readonly IContextualIdProvider _contextualIdProvider;
        private readonly IStackTraceParser _stackTraceParser;
        private readonly IClientConfiguration _clientConfiguration;
        private readonly PeriodicDispatcher _periodicDispatcher;
        
        public MicroserviceAnalyticClient(string propertyId,
            string key,
            IEnvironmentCapture environmentCapture,
            IContextualIdProvider contextualIdProvider,
            IStackTraceParser stackTraceParser,
            IClientConfiguration clientConfiguration)
        {
            _environmentCapture = environmentCapture;
            _contextualIdProvider = contextualIdProvider;
            _stackTraceParser = stackTraceParser;
            _clientConfiguration = clientConfiguration;

            _periodicDispatcher = new PeriodicDispatcher(this, propertyId, key, clientConfiguration);
        }

        public IClientConfiguration ClientConfiguration => _clientConfiguration;

        public IReadOnlyCollection<Event> GetEvents(int batchSize)
        {
            List<Event> events = new List<Event>();
            Event ev;
            while (_eventQueue.TryDequeue(out ev) && events.Count < batchSize)
            {
                events.Add(ev);
            }
            return events;
        }

        public Environment GetEnvironment()
        {
            return _environmentCapture.Get();
        }

        public void SqlCommand(
            DateTimeOffset executedAt,
            string connectionString,
            string commandText,
            int durationInMilliseconds,
            bool succeeded,
            int? sqlErrorCode)
        {
            if (!_clientConfiguration.IsCaptureSqlEnabled)
                return;

            Event ev = new Event
            {
                CorrelationIds = string.IsNullOrWhiteSpace(_contextualIdProvider.CorrelationId) ? new List<string>() : new List<string>() { _contextualIdProvider.CorrelationId },
                SessionId = _contextualIdProvider.SessionId,
                UserId = _contextualIdProvider.UserId,
                Data = new Dictionary<string, object>
                {
                    {"CommandText", commandText },
                    {"ConnectionString", connectionString },
                    {"Succeeded", succeeded.ToString() },
                    {"DurationInMilliseconds", durationInMilliseconds.ToString() }
                },
                EventStartDateTime = executedAt.ToString(Event.EventDateFormat, CultureInfo.InvariantCulture),
                EventType = EventTypes.SqlCommand
            };

            if (sqlErrorCode.HasValue)
            {
                ev.Data.Add("SqlErrorCode", sqlErrorCode.Value.ToString());
            }

            _eventQueue.Enqueue(ev);
        }

        public void Indicator(string indicatorType, double value)
        {
            if (!_clientConfiguration.IsCaptureCustomMetricEnabled)
                return;

            Event ev = new Event
            {
                CorrelationIds = string.IsNullOrWhiteSpace(_contextualIdProvider.CorrelationId) ? new List<string>() : new List<string>() { _contextualIdProvider.CorrelationId },
                SessionId = _contextualIdProvider.SessionId,
                UserId = _contextualIdProvider.UserId,
                Data = new Dictionary<string, object>
                {
                    {"MetricType", indicatorType },
                    {"Value", value }
                },
                EventStartDateTime = DateTimeOffset.UtcNow.ToString(Event.EventDateFormat, CultureInfo.InvariantCulture),
                EventType = EventTypes.CustomMetric
            };

            _eventQueue.Enqueue(ev);
        }

        public void Log(string message, int levelRank, string levelText, DateTimeOffset timestamp, Exception ex, object payload)
        {
            if (!_clientConfiguration.IsCaptureLogsEnabled)
                return;

            Event ev = new Event
            {
                CorrelationIds = string.IsNullOrWhiteSpace(_contextualIdProvider.CorrelationId) ? new List<string>() : new List<string>() { _contextualIdProvider.CorrelationId },
                SessionId = _contextualIdProvider.SessionId,
                UserId = _contextualIdProvider.UserId,
                Data = new Dictionary<string, object>
                {
                    {"Level", levelRank },
                    {"LevelText", levelText },
                    {"Message", message },
                    {"Payload", payload }
                },
                EventEndDateTime = null,
                EventStartDateTime = timestamp.ToString(Event.EventDateFormat),
                EventType = EventTypes.Log
            };

            if (ex != null)
            {
                ev.Data["ExceptionMessage"] = ex.Message;
                ev.Data["ExceptionType"] = ex.GetType().FullName;
                ev.Data["ExceptionStackFrames"] = _stackTraceParser.Get(ex);
            }

            _eventQueue.Enqueue(ev);
        }

        public void LogWithJson(string message, int levelRank, string levelText, DateTimeOffset timestamp, Exception ex, string jsonPayload)
        {
            object payload = JsonConvert.DeserializeObject(jsonPayload);
            Log(message, levelRank, levelText, timestamp, ex, payload);
        }

        public void Error(Exception ex, Dictionary<string, string> additionalData = null, string correlationId = null, string sessionId = null, string userId = null)
        {
            if (!_clientConfiguration.IsCaptureErrorsEnabled)
                return;

            List<string> correlationIds = GetCorrelationIdList(correlationId);
            Event ev = new Event
            {
                CorrelationIds = correlationIds,
                SessionId = String.IsNullOrWhiteSpace(sessionId) ? _contextualIdProvider.SessionId : sessionId,
                UserId = String.IsNullOrWhiteSpace(userId) ? _contextualIdProvider.UserId : userId,
                Data = new Dictionary<string, object>
                {
                    {"Message", ex.Message },
                    {"ExceptionType",ex.GetType().FullName },
                    {"StackFrames", _stackTraceParser.Get(ex) }
                },
                EventEndDateTime = null,
                EventStartDateTime = DateTimeOffset.UtcNow.ToString(Event.EventDateFormat),
                EventType = EventTypes.Error
            };

            _eventQueue.Enqueue(ev);
        }

        public void HttpRequest(string verb,
            int statusCode,
            string uri,
            bool didStripQueryParams,
            string correlationId,
            string sessionId,
            string userId,
            DateTimeOffset requestDateTime,
            long durationInMilliseconds,
            Dictionary<string, string[]> requestHeaders,
            Dictionary<string, string[]> responseHeaders)
        {
            if (!_clientConfiguration.IsCaptureHttpEnabled)
                return;
            if (_clientConfiguration.ExcludedVerbs.Contains(verb.ToUpper()))
                return;

            List<string> correlationIds = GetCorrelationIdList(correlationId);
            Event ev = new Event
            {
                CorrelationIds = correlationIds,
                SessionId = sessionId,
                UserId = userId,
                Data = new Dictionary<string, object>
                {
                    {"Verb", verb },
                    {"StatusCode", statusCode },
                    {"Uri", uri },
                    {"DidStripQueryParams", didStripQueryParams },
                    {"DurationInMilliseconds", durationInMilliseconds },
                    {"RequestHeaders",requestHeaders },
                    {"ResponseHeaders", responseHeaders }
                },
                EventEndDateTime = null,
                EventStartDateTime = requestDateTime.ToString(Event.EventDateFormat),
                EventType = EventTypes.HttpTrace
            };
            _eventQueue.Enqueue(ev);
        }

        private List<string> GetCorrelationIdList(string correlationId)
        {
            List<string> correlationIds;
            if (correlationId != null)
            {
                correlationIds = new List<string> { correlationId };
            }
            else
            {
                correlationIds = new List<string> { _contextualIdProvider.CorrelationId };
            }
            return correlationIds;
        }
    }
}
