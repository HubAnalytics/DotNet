using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HubAnalytics.Core.Helpers;
using HubAnalytics.Core.Model;

namespace HubAnalytics.Core.Implementation
{
    internal class PeriodicDispatcher
    {
        private const int TooManyRequests = 429;
        private const string EventRelativePath = "v1/event";
        private const int MaxBatchSize = 200;
        
        private readonly IHubAnalyticsClient _hubAnalyticsClient;
        private readonly string _propertyId;
        private readonly string _key;
        private readonly IClientConfiguration _clientConfiguration;
        private readonly IJsonSerialization _jsonSerialization;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Uri _endpoint;

        public PeriodicDispatcher(IHubAnalyticsClient hubAnalyticsClient,
            string propertyId,
            string key,
            IClientConfiguration clientConfiguration,
            IJsonSerialization jsonSerialization,
            CancellationTokenSource cancellationTokenSource)
        {
            _hubAnalyticsClient = hubAnalyticsClient;
            _propertyId = propertyId;
            _key = key;
            _clientConfiguration = clientConfiguration;
            _jsonSerialization = jsonSerialization;
            _cancellationTokenSource = cancellationTokenSource;
            string apiRoot = clientConfiguration.ApiRoot;
            _endpoint = new Uri(apiRoot.EndsWith("/") ? $"{apiRoot}{EventRelativePath}" : $"{apiRoot}/{EventRelativePath}");
            
            Task.Run(async () =>
            {
                await BackgroundPush();
            });
        }

        private string SerializeObject(object value)
        {
            return _jsonSerialization.Serialize(value);
        }

        private async Task BackgroundPush()
        {
            bool shouldDelay = true;
            bool shouldContinue = true;
            bool shouldDispatch = true;
            DateTimeOffset? attemptReconnectAt = null;

            while (shouldContinue)
            {
                if (shouldDelay)
                {
                    await Task.Delay(_clientConfiguration.UploadInterval);
                }

                if (attemptReconnectAt.HasValue && attemptReconnectAt.Value < DateTimeOffset.UtcNow)
                {
                    shouldDispatch = true;
                }
                
                try
                {
                    IReadOnlyCollection<Event> events = _hubAnalyticsClient.GetEvents(MaxBatchSize);
                    if (events!= null && events.Any())
                    {
                        if (shouldDispatch)
                        {
                            string applicationVersion = _clientConfiguration.ApplicationVersion;
                            if (string.IsNullOrWhiteSpace(applicationVersion))
                            {
                                applicationVersion = "0.0.0";
                            }
                            EventBatch batch = new EventBatch
                            {
                                ApplicationVersion = applicationVersion,
                                Environment = _hubAnalyticsClient.GetEnvironment(),
                                Events = events.ToList(),
                                Source = "net"
                            };

                            //string serializedContent = Newtonsoft.Json.JsonConvert.SerializeObject(batch);

                            string serializedContent = SerializeObject(batch);
                            HttpClient client = new HttpClient();
                            HttpContent content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
                            HttpRequestMessage message = new HttpRequestMessage
                            {
                                Content = content,
                                Method = HttpMethod.Post,
                                RequestUri = _endpoint
                            };
                            message.Headers.Add("af-property-id", _propertyId);
                            message.Headers.Add("af-collection-key", _key);

                            using (HttpResponseMessage response = await client.SendAsync(message))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    await response.Content.ReadAsStringAsync();
                                }
                                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    // on an unauthorized request we stop dispatching to the server but we continue to flush
                                    // the event queue preventing memory from being gradually consumed
                                    shouldDispatch = false;
                                    _cancellationTokenSource.Cancel();
                                }
                                else if ((int) response.StatusCode == TooManyRequests)
                                {
                                    // if we've hit the subscription limit then we stop dispatching to the server but leave a marker
                                    // to try and reestablish contact in 15 minutes to pick up again if the rate limit is removed on the
                                    // server (normally due to falling back under the subscription limits or an increase in the subscription
                                    // plan)
                                    shouldDispatch = false;
                                    attemptReconnectAt = DateTimeOffset.UtcNow.AddMinutes(15);
                                }
                            }
                        }
                        
                        shouldDelay = events.Count >= MaxBatchSize;
                    }
                    else
                    {
                        shouldDelay = true;
                    }
                }
                catch (Exception)
                {
                    shouldDelay = true;
                }
            }
        }
    }
}
