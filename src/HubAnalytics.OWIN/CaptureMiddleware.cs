using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HubAnalytics.Core;
using Microsoft.Owin;

namespace HubAnalytics.OWIN
{
    public class CaptureMiddleware : OwinMiddleware
    {
        private readonly IHubAnalyticsClient _hubAnalyticsClient;
        private readonly IContextualIdProvider _contextualIdProvider;
        private readonly IClientConfiguration _clientConfiguration;

        public CaptureMiddleware(OwinMiddleware next, HubAnalyticsClientFactory hubAnalyticsClientFactory = null) : base(next)
        {
            if (hubAnalyticsClientFactory == null)
            {
                hubAnalyticsClientFactory = new HubAnalyticsClientFactory();
            }
            _hubAnalyticsClient = hubAnalyticsClientFactory.GetClient();
            _contextualIdProvider = hubAnalyticsClientFactory.GetCorrelationIdProvider();
            _clientConfiguration = hubAnalyticsClientFactory.GetClientConfiguration();
        }

        public override async Task Invoke(IOwinContext context)
        {
            DateTimeOffset requestDateTime = DateTimeOffset.UtcNow;
            Stopwatch sw = Stopwatch.StartNew();
            string correlationId = EnsureCorrelation(context);
            string sessionId = EnsureSessionId(context);
            string userId = EnsureUserId(context);
            context.Response.OnSendingHeaders(state =>
            {
                OwinResponse localResponse = (OwinResponse)state;
                EnsureContextualIdsOnResponse(localResponse, correlationId, sessionId, userId);
            }, context.Response);
            try
            {
                await Next.Invoke(context);
                sw.Stop();
            }
            catch (Exception ex)
            {
                _hubAnalyticsClient.Error(ex, null, correlationId);
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

            _hubAnalyticsClient.HttpRequest(
                context.Request.Method,
                context.Response.StatusCode,
                uri,
                didStripQueryParams,
                correlationId,
                sessionId,
                userId,
                requestDateTime,
                sw.ElapsedMilliseconds,
                CaptureHeaders(_clientConfiguration.HttpRequestHeaderWhitelist, context.Request.Headers),
                CaptureHeaders(_clientConfiguration.HttpResponseHeaderWhitelist, context.Response.Headers)
                );
        }

        private void EnsureContextualIdsOnResponse(OwinResponse response, string correlationId, string sessionId, string userId)
        {
            if (!response.Headers.ContainsKey(_clientConfiguration.CorrelationIdKey))
            {
                response.Headers.Add(_clientConfiguration.CorrelationIdKey, new[] { correlationId });
            }
            if (!response.Headers.ContainsKey(_clientConfiguration.SessionIdKey) && !string.IsNullOrWhiteSpace(sessionId))
            {
                response.Headers.Add(_clientConfiguration.SessionIdKey, new[] { sessionId });
            }
            if (!response.Headers.ContainsKey(_clientConfiguration.UserIdKey) && !string.IsNullOrWhiteSpace(userId))
            {
                response.Headers.Add(_clientConfiguration.UserIdKey, new[] { userId });
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
            _contextualIdProvider.CorrelationId = correlationId;
            return correlationId;
        }

        private string EnsureSessionId(IOwinContext context)
        {
            string sessionId = context.Request.Headers[_clientConfiguration.SessionIdKey];
            _contextualIdProvider.SessionId = sessionId;
            return sessionId;
        }

        private string EnsureUserId(IOwinContext context)
        {
            string userId = context.Request.Headers[_clientConfiguration.UserIdKey];
            _contextualIdProvider.SessionId = userId;
            return userId;
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
