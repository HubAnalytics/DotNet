using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceAnalytics.Core.Model;

namespace MicroserviceAnalytics.Core.Implementation
{
    internal class PeriodicDispatcher
    {
        private const string EventRelativePath = "v1/event";
        private const int MaxBatchSize = 200;
        
        private readonly IMicroserviceAnalyticClient _microserviceAnalyticClient;
        private readonly string _propertyId;
        private readonly string _key;
        private readonly IClientConfiguration _clientConfiguration;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Uri _endpoint;

        public PeriodicDispatcher(IMicroserviceAnalyticClient microserviceAnalyticClient,
            string propertyId,
            string key,
            IClientConfiguration clientConfiguration,
            CancellationTokenSource cancellationTokenSource)
        {
            _microserviceAnalyticClient = microserviceAnalyticClient;
            _propertyId = propertyId;
            _key = key;
            _clientConfiguration = clientConfiguration;
            _cancellationTokenSource = cancellationTokenSource;
            string apiRoot = clientConfiguration.ApiRoot;
            _endpoint = new Uri(apiRoot.EndsWith("/") ? $"{apiRoot}{EventRelativePath}" : $"{apiRoot}/{EventRelativePath}");
            
            Task.Run(async () =>
            {
                await BackgroundPush();
            });
        }

        private async Task BackgroundPush()
        {
            bool shouldDelay = true;
            bool shouldContinue = true;
            while (shouldContinue)
            {
                if (shouldDelay)
                {
                    await Task.Delay(_clientConfiguration.UploadInterval);
                }
                
                try
                {
                    IReadOnlyCollection<Event> events = _microserviceAnalyticClient.GetEvents(MaxBatchSize);
                    if (events!= null && events.Any())
                    {
                        EventBatch batch = new EventBatch
                        {
                            ApplicationVersion = "1.0.0.0",
                            Environment = _microserviceAnalyticClient.GetEnvironment(),
                            Events = events.ToList(),
                            Source = "net"
                        };

                        string serializedContent = Newtonsoft.Json.JsonConvert.SerializeObject(batch);
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
                                shouldContinue = false;
                                _cancellationTokenSource.Cancel();
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
