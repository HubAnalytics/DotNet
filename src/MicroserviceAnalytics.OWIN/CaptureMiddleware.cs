using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceAnalytics.Core;
using Microsoft.Owin;

namespace MicroserviceAnalytics.Owin
{
    public class CaptureMiddleware : OwinMiddleware
    {
        private readonly IMicroserviceAnalyticClient _microserviceAnalyticClient;
        private readonly ICorrelationIdProvider _correlationIdProvider;
        private readonly IClientConfiguration _clientConfiguration;

        public CaptureMiddleware(OwinMiddleware next, MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory = null) : base(next)
        {
            if (microserviceAnalyticClientFactory == null)
            {
                microserviceAnalyticClientFactory = new MicroserviceAnalyticClientFactory();
            }
            _microserviceAnalyticClient = microserviceAnalyticClientFactory.GetClient();
            _correlationIdProvider = microserviceAnalyticClientFactory.GetCorrelationIdProvider();
            _clientConfiguration = microserviceAnalyticClientFactory.GetClientConfiguration();
        }

        public override async Task Invoke(IOwinContext context)
        {
            DateTimeOffset requestDateTime = DateTimeOffset.UtcNow;
            Stopwatch sw = Stopwatch.StartNew();
            string correlationId = EnsureCorrelation(context);
            context.Response.OnSendingHeaders(state =>
            {
                OwinResponse localResponse = (OwinResponse)state;
                EnsureCorrelationOnResponse(localResponse, correlationId);
            }, context.Response);
            try
            {
                await Next.Invoke(context);
                sw.Stop();
            }
            catch (Exception ex)
            {
                _microserviceAnalyticClient.Error(ex, null, correlationId);
            }

            string uri = context.Request.Uri.ToString();
            bool didStripQueryParams = false;
            if (_clientConfiguration.StripHttpQueryParams)
            {
                int index = uri.IndexOf("?", StringComparison.Ordinal);
                if (index >= 0)
                {
                    uri = uri.Substring(0, index);
                    didStripQueryParams = true;
                }
            }

            _microserviceAnalyticClient.HttpRequest(
                context.Request.Method,
                context.Response.StatusCode,
                uri,
                didStripQueryParams,
                correlationId,
                requestDateTime,
                sw.ElapsedMilliseconds,
                CaptureHeaders(_clientConfiguration.HttpRequestHeaderWhitelist, context.Request.Headers),
                CaptureHeaders(_clientConfiguration.HttpResponseHeaderWhitelist, context.Response.Headers)
                );
        }

        private void EnsureCorrelationOnResponse(OwinResponse response, string correlationId)
        {
            if (!response.Headers.ContainsKey(_clientConfiguration.CorrelationIdKey))
            {
                response.Headers.Add(_clientConfiguration.CorrelationIdKey, new[] { correlationId });
            }
        }

        private string EnsureCorrelation(IOwinContext context)
        {
            string correlationId;
            string value = context.Request.Headers[_clientConfiguration.CorrelationIdKey];
            if (!string.IsNullOrWhiteSpace(value))
            {
                correlationId = value;
            }
            else
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers.Add(_clientConfiguration.CorrelationIdKey, new[] {correlationId});
            }
            _correlationIdProvider.CorrelationId = correlationId;
            return correlationId;
        }

        private Dictionary<string, string[]> CaptureHeaders(IReadOnlyCollection<string> captureHeaders, IHeaderDictionary headers)
        {
            Dictionary<string, string[]> capturedHeaders = null;
            if (captureHeaders != null && captureHeaders.Any())
            {
                capturedHeaders = new Dictionary<string, string[]>();
                if (captureHeaders.First() == "*")
                {
                    foreach (KeyValuePair<string, string[]> kvp in headers)
                    {
                        capturedHeaders[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, string[]> kvp in headers)
                    {
                        if (captureHeaders.Contains(kvp.Key))
                        {
                            capturedHeaders.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
            }
            return capturedHeaders;
        }
    }
}
