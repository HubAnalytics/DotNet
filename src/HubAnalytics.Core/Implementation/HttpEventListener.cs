#if !DNXCORE50
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using HubAnalytics.Core.Helpers;

namespace HubAnalytics.Core.Implementation
{
    public class HttpEventListener : EventListener
    {
        private readonly IHubAnalyticsClient _client;
        private readonly IContextualIdProvider _contextualIdProvider;

        class HttpEvent
        {
            public Stopwatch Stopwatch { get; set; }

            public string Url { get; set; }

            public DateTimeOffset RequestedAt { get; set; }

            public string CorrelationId { get; set; }

            public string SessionId { get; set; }

            public string UserId { get; set; }
        }

        private const int HttpBeginResponse = 140;
        private const int HttpEndResponse = 141;
        private const int HttpBeginGetRequestStream = 142;
        private const int HttpEndGetRequestStream = 143;

        private readonly ConcurrentDictionary<long, HttpEvent> _trackedEvents = new ConcurrentDictionary<long, HttpEvent>();

        public HttpEventListener(IHubAnalyticsClient client, IContextualIdProvider contextualIdProvider)
        {
            _client = client;
            _contextualIdProvider = contextualIdProvider;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource != null && eventSource.Name == "System.Diagnostics.Eventing.FrameworkEventSource")
            {
                EnableEvents(eventSource, EventLevel.Informational, (EventKeywords)4);
            }
            base.OnEventSourceCreated(eventSource);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData?.Payload == null)
                return;

            try
            {
                switch (eventData.EventId)
                {
                    case HttpBeginResponse:
                        OnBeginHttpResponse(eventData);
                        break;
                    case HttpEndResponse:
                        OnEndHttpResponse(eventData);
                        break;
                    case HttpBeginGetRequestStream:
                        OnBeginHttpResponse(eventData);
                        break;
                    case HttpEndGetRequestStream:
                        OnEndHttpResponse(eventData);
                        break;
                }
            }
            catch (Exception)
            {
                // something odd happened, don't allow analytics to disturb normal execution
            }
        }

        private void OnBeginHttpResponse(EventWrittenEventArgs httpEventData)
        {
            if (httpEventData.Payload.Count < 2)
            {
                return;
            }
#if NET46
            int indexOfId = httpEventData.PayloadNames.IndexOf("id");
            int indexOfUrl = httpEventData.PayloadNames.IndexOf("uri");
#else
            int indexOfId = 0;
            int indexOfUrl = 1;
#endif

            if (indexOfId == -1 || indexOfUrl == -1)
            {
                return;
            }
            long id = Convert.ToInt64(httpEventData.Payload[indexOfId], CultureInfo.InvariantCulture);
            string url = Convert.ToString(httpEventData.Payload[indexOfUrl], CultureInfo.InvariantCulture);
            _trackedEvents[id] = new HttpEvent
            {
                Url = url,
                Stopwatch = new Stopwatch(),
                RequestedAt = DateTimeOffset.UtcNow,
                CorrelationId = _contextualIdProvider.CorrelationId,
                UserId = _contextualIdProvider.UserId,
                SessionId = _contextualIdProvider.SessionId
            };
            _trackedEvents[id].Stopwatch.Start();
        }

        private void OnEndHttpResponse(EventWrittenEventArgs httpEventData)
        {
            if (httpEventData.Payload.Count < 1)
            {
                return;
            }
#if NET46
            int indexOfId = httpEventData.PayloadNames.IndexOf("id");
            if (indexOfId == -1)
            {
                return;
            }
#else
            int indexOfId = 0;
#endif
            long id = Convert.ToInt64(httpEventData.Payload[indexOfId], CultureInfo.InvariantCulture);
            HttpEvent trackedEvent;
            if (_trackedEvents.TryRemove(id, out trackedEvent))
            {
                trackedEvent.Stopwatch.Stop();
#if NET46
                int indexOfSuccess = httpEventData.PayloadNames.IndexOf("success");
                int indexOfSynchronous = httpEventData.PayloadNames.IndexOf("synchronous");
                int indexOfStatusCode = httpEventData.PayloadNames.IndexOf("statusCode");
#else
                int indexOfSuccess = httpEventData.Payload.Count > 1 ? 1 : -1;
                int indexOfSynchronous = httpEventData.Payload.Count > 2 ? 2 : -1;
                int indexOfStatusCode = httpEventData.Payload.Count > 3 ? 3 : -1;
#endif

                bool? success = indexOfSuccess > -1 ? new bool?(Convert.ToBoolean(httpEventData.Payload[indexOfSuccess])) : null;
                bool? synchronous = indexOfSynchronous > -1 ? new bool?(Convert.ToBoolean(httpEventData.Payload[indexOfSynchronous])) : null;
                int? statusCode = indexOfStatusCode > -1 ? new int?(Convert.ToInt32(httpEventData.Payload[indexOfStatusCode])) : null;

                _client.ExternalHttpRequest(trackedEvent.Url,
                    trackedEvent.RequestedAt,
                    trackedEvent.Stopwatch.ElapsedMilliseconds,
                    trackedEvent.CorrelationId,
                    trackedEvent.UserId,
                    trackedEvent.SessionId,
                    success,
                    synchronous,
                    statusCode);                
            }
        }
    }
}
#endif