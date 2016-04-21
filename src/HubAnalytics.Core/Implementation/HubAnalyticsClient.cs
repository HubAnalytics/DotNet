using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using HubAnalytics.Core.Helpers;
using HubAnalytics.Core.Model;
using Newtonsoft.Json;
using Environment = HubAnalytics.Core.Model.Environment;

namespace HubAnalytics.Core.Implementation
{
    internal class HubAnalyticsClient : IHubAnalyticsClient
    {
        private readonly ConcurrentBag<ITelemetryEventProvider> _telemetryEventProviders = new ConcurrentBag<ITelemetryEventProvider>();
        private readonly ConcurrentQueue<Event> _eventQueue = new ConcurrentQueue<Event>();
        private readonly IEnvironmentCapture _environmentCapture;
        private readonly IContextualIdProvider _contextualIdProvider;
        private readonly IStackTraceParser _stackTraceParser;
        private readonly IClientConfiguration _clientConfiguration;
        private readonly PeriodicDispatcher _periodicDispatcher;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        public HubAnalyticsClient(string propertyId,
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
            _cancellationTokenSource = new CancellationTokenSource();

            _periodicDispatcher = new PeriodicDispatcher(this, propertyId, key, clientConfiguration, _cancellationTokenSource);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public void RegisterTelemetryProvider(ITelemetryEventProvider telemetryEventProvider)
        {
            _telemetryEventProviders.Add(telemetryEventProvider);
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

            var telemetryProviders = _telemetryEventProviders.ToArray();
            foreach (ITelemetryEventProvider provider in telemetryProviders)
            {
                events.AddRange(provider.GetEvents(batchSize));
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
            if (!_clientConfiguration.IsCaptureSqlEnabled || _cancellationTokenSource.IsCancellationRequested)
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
                EventStartDateTime = executedAt.ToUniversalTime().ToString(Event.EventDateFormat, CultureInfo.InvariantCulture),
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
            if (!_clientConfiguration.IsCaptureCustomMetricEnabled || _cancellationTokenSource.IsCancellationRequested)
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
            if (!_clientConfiguration.IsCaptureLogsEnabled || _cancellationTokenSource.IsCancellationRequested)
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
                EventStartDateTime = timestamp.ToUniversalTime().ToString(Event.EventDateFormat),
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
            if (!_clientConfiguration.IsCaptureLogsEnabled || _cancellationTokenSource.IsCancellationRequested)
                return;

            object payload = JsonConvert.DeserializeObject(jsonPayload);
            Log(message, levelRank, levelText, timestamp, ex, payload);
        }

        public void Error(Exception ex, Dictionary<string, string> additionalData = null, string correlationId = null, string sessionId = null, string userId = null)
        {
            if (!_clientConfiguration.IsCaptureErrorsEnabled || _cancellationTokenSource.IsCancellationRequested)
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
            if (_cancellationTokenSource.IsCancellationRequested)
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
                EventStartDateTime = requestDateTime.ToUniversalTime().ToString(Event.EventDateFormat),
                EventType = EventTypes.HttpTrace
            };
            _eventQueue.Enqueue(ev);
        }

        private List<string> GetCorrelationIdList(string correlationId)
        {
            List<string> correlationIds = correlationId != null ? new List<string> { correlationId } : new List<string> { _contextualIdProvider.CorrelationId };
            return correlationIds;
        }
    }
}
