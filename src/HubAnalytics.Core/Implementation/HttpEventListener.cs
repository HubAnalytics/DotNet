#if !DNXCORE50
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using HubAnalytics.Core.Helpers;
using HubAnalytics.Core.Model;

namespace HubAnalytics.Core.Implementation
{
    public class HttpEventListener : EventListener
    {
        private readonly IHubAnalyticsClient _client;
        private readonly IReadOnlyCollection<IHttpEventUrlParser> _urlParsers;
        private readonly IUrlProcessor _urlProcessor;

        class HttpEvent
        {
            public Stopwatch Stopwatch { get; set; }

            public string Url { get; set; }

            public DateTimeOffset RequestedAt { get; set; }            
        }

        private const int HttpBeginResponse = 140;
        private const int HttpEndResponse = 141;
        private const int HttpBeginGetRequestStream = 142;
        private const int HttpEndGetRequestStream = 143;

        private readonly ConcurrentDictionary<long, HttpEvent> _trackedEvents = new ConcurrentDictionary<long, HttpEvent>();

        public HttpEventListener(IHubAnalyticsClient client, IReadOnlyCollection<IHttpEventUrlParser> urlParsers, IUrlProcessor urlProcessor)
        {
            _client = client;
            _urlParsers = urlParsers;
            _urlProcessor = urlProcessor;
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
                    case HttpBeginGetRequestStream:
                        OnBeginHttpEvent(eventData);
                        break;
                    case HttpEndResponse:
                        OnEndHttpEvent(eventData);
                        break;
                }
            }
            catch (Exception)
            {
                // something odd happened, don't allow analytics to disturb normal execution
            }
        }

        private void OnBeginHttpEvent(EventWrittenEventArgs httpEventData)
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

            // don't log external requests to the analytic end point
            if (url.StartsWith(_client.ClientConfiguration.ApiRoot))
            {
                return;
            }

            if (_urlProcessor != null)
            {
                url = _urlProcessor.Process(url);
                if (string.IsNullOrWhiteSpace(url))
                {
                    return;
                }
            }

            HttpEvent httpEvent = new HttpEvent
            {
                Url = url,
                Stopwatch = new Stopwatch(),
                RequestedAt = DateTimeOffset.UtcNow                
            };
            if (_trackedEvents.TryAdd(id, httpEvent))
            {
                httpEvent.Stopwatch.Start();
            }            
        }

        private void OnEndHttpEvent(EventWrittenEventArgs httpEventData)
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
                
                foreach (IHttpEventUrlParser urlParser in _urlParsers)
                {
                    string type;
                    string domain;
                    string name;
                    if (urlParser.Parse(trackedEvent.Url, out domain, out name, out type))
                    {
                        _client.ExternalHttpRequest(trackedEvent.RequestedAt,
                            trackedEvent.Stopwatch.ElapsedMilliseconds,
                            success,
                            name,
                            domain,
                            type);
                        break;
                    }
                }                
            }
        }

        public void Cancel()
        {
            
        }

        public IReadOnlyCollection<Event> GetEvents(int batchSize)
        {
            throw new NotImplementedException();
        }
    }
}
#endif