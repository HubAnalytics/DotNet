using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using HubAnalytics.AspNet4.Implementation;
using HubAnalytics.Core;

namespace HubAnalytics.AspNet4
{
    public class DataCollectionHttpModule : IHttpModule
    {
        private readonly IHeaderParser _headerParser;
        private readonly IHubAnalyticsClient _hubAnalyticsClient;
        private readonly IContextualIdProvider _contextualIdProvider;
        private readonly IUserIdProvider _userIdProvider;
        private readonly ISessionIdProvider _sessionIdProvider;
        
        public DataCollectionHttpModule() : this(null, null)
        {
            
        }

        public DataCollectionHttpModule(IHubAnalyticsClientFactory hubAnalyticsClientFactory, IHeaderParser headerParser)
        {
            if (hubAnalyticsClientFactory == null)
            {
                hubAnalyticsClientFactory = new HubAnalyticsClientFactory();
            }
            if (headerParser == null)
            {
                headerParser = new HeaderParser();
            }
            IClientConfiguration clientConfiguration = hubAnalyticsClientFactory.GetClientConfiguration();
            _headerParser = headerParser;
            _hubAnalyticsClient = hubAnalyticsClientFactory.GetClient();
            _contextualIdProvider = hubAnalyticsClientFactory.GetCorrelationIdProvider();
            _sessionIdProvider = clientConfiguration.IsSessionIdCreationEnabled ? hubAnalyticsClientFactory.GetRuntimeProviderDiscoveryService().SessionIdProvider : null;
            _userIdProvider = clientConfiguration.IsSessionIdCreationEnabled ? hubAnalyticsClientFactory.GetRuntimeProviderDiscoveryService().UserIdProvider : null;
        }

        public void Init(HttpApplication context)
        {
            context.Error += CatchError;
            context.BeginRequest += BeginRequest;
            context.PreSendRequestHeaders += AppendRequestHeaders;
            context.EndRequest += EndRequest;
        }

        private void AppendRequestHeaders(object sender, EventArgs e)
        {
            try
            {
                SetCorrelationIdOnResponse(sender);
                SetSessionAndUserIdOnResponse(sender);
            }
            catch (Exception ex)
            {
                _hubAnalyticsClient.Error(ex);
            }

        }

        private void EndRequest(object sender, EventArgs e)
        {
            try
            {
                if (IsCaptureHttpEnabled)
                {
                    EndHttpTrace(sender);
                }
            }
            catch (Exception ex)
            {
                _hubAnalyticsClient.Error(ex);
            }
        }
        
        private void BeginRequest(object sender, EventArgs e)
        {
            try
            {
                SetContextualIdsOnRequest(sender);
                if (IsCaptureHttpEnabled)
                {
                    BeginHttpTrace(sender);
                }
            }
            catch (Exception ex)
            {
                _hubAnalyticsClient.Error(ex);
            }
            
        }

        private void CatchError(object sender, EventArgs e)
        {
            try
            {
                HttpApplication application = sender as HttpApplication;
                if (application != null)
                {
                    Exception ex = application.Server.GetLastError();
                    _hubAnalyticsClient.Error(ex);
                }
            }
            catch (Exception)
            {
                // do nothing
            }
            
        }

        public void Dispose()
        {
            
        }

        private void BeginHttpTrace(object sender)
        {
            HttpApplication application = (HttpApplication)sender;
            Stopwatch sw = new Stopwatch();
            application.Context.Items[StopwatchKey] = sw;
            sw.Start();
        }

        private void EndHttpTrace(object sender)
        {
            HttpApplication application = (HttpApplication)sender;
            
            long elapsedMilliseconds = 0;
            Stopwatch sw = (Stopwatch) application.Context.Items[StopwatchKey];
            if (sw != null)
            {
                sw.Stop();
                elapsedMilliseconds = sw.ElapsedMilliseconds;
            }
            
            HttpRequest request = application.Request;
            
            string uri = request.Url.ToString();
            bool didStripQueryParams = false;
            if (StripQueryParams && request.QueryString.Count > 0)
            {
                uri = uri.Substring(0, uri.IndexOf("?", StringComparison.Ordinal));
                didStripQueryParams = true;
            }
            string verb = request.HttpMethod;
            string correlationId = GetCorrelationId(request);
            string userId = GetUserId(application);
            string sessionId = GetSessionId(application);
            Dictionary<string, string[]> requestHeaders = _headerParser.CaptureHeaders(CapturedRequestHeaders, request.Headers);
            Dictionary<string, string[]> responseHeaders = _headerParser.CaptureHeaders(CapturedResponseHeaders, HttpContext.Current.Response.Headers);

            _hubAnalyticsClient.HttpRequest(verb,
                HttpContext.Current.Response.StatusCode,
                uri,
                didStripQueryParams,
                correlationId,
                sessionId,
                userId,
                DateTimeOffset.UtcNow.AddMilliseconds(-elapsedMilliseconds),
                elapsedMilliseconds,
                requestHeaders,
                responseHeaders);
        }

        private void SetContextualIdsOnRequest(object sender)
        {
            HttpApplication application = (HttpApplication)sender;
            string correlationId;
            string userId = null;
            string sessionId = null;
            string[] values;

            if (_hubAnalyticsClient.ClientConfiguration.EnableCorrelation)
            {
                values = application.Request.Headers.GetValues(CorrelationIdKey);
                if (values != null && values.Any())
                {
                    correlationId = values[0];
                }
                else
                {
                    correlationId = Guid.NewGuid().ToString();
                    application.Request.Headers.Add(CorrelationIdKey, correlationId);
                }
                _contextualIdProvider.CorrelationId = correlationId;
            }

            if (_hubAnalyticsClient.ClientConfiguration.IsSessionTrackingEnabled)
            {
                values = application.Request.Headers.GetValues(SessionIdKey);
                if (values != null && values.Any())
                {
                    sessionId = values[0];
                }
                _contextualIdProvider.SessionId = sessionId;
            }

            if (_hubAnalyticsClient.ClientConfiguration.IsUserTrackingEnabled)
            {
                values = application.Request.Headers.GetValues(UserIdKey);
                if (values != null && values.Any())
                {
                    userId = values[0];
                    _contextualIdProvider.UserId = userId;
                }
            }            
        }

        private void SetCorrelationIdOnResponse(object sender)
        {
            if (!_hubAnalyticsClient.ClientConfiguration.EnableCorrelation) return;

            HttpApplication application = (HttpApplication)sender;

            string[] responseValues = application.Response.Headers.GetValues(CorrelationIdKey);
            if (responseValues == null || !responseValues.Any())
            {
                HttpRequest request = application.Request;
                var correlationId = GetCorrelationId(request);

                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    application.Response.Headers.Add(CorrelationIdKey, correlationId);
                }
            }
        }

        private void SetSessionAndUserIdOnResponse(object sender)
        {
            HttpApplication application = (HttpApplication)sender;

            string[] responseValues = application.Response.Headers.GetValues(SessionIdKey);
            if (responseValues == null || !responseValues.Any())
            {
                var sessionId = GetSessionId(application);
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    application.Response.Headers.Add(SessionIdKey, sessionId);
                }
            }
            responseValues = application.Response.Headers.GetValues(UserIdKey);
            if (responseValues == null || !responseValues.Any())
            {
                var userId = GetUserId(application);
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    application.Response.Headers.Add(UserIdKey, userId);
                }
            }
        }

        private string GetCorrelationId(HttpRequest request)
        {
            string correlationId;
            string[] headerCorrelationIdValues = request.Headers.GetValues(CorrelationIdKey);
            if (headerCorrelationIdValues != null && headerCorrelationIdValues.Any())
            {
                correlationId = headerCorrelationIdValues.First();
            }
            else
            {
                correlationId = _contextualIdProvider.CorrelationId;
            }
            return correlationId;
        }

        private string GetSessionId(HttpApplication application)
        {
            string sessionId = null;
            string[] headerSessionIdValues = application.Request.Headers.GetValues(SessionIdKey);
            if (headerSessionIdValues != null && headerSessionIdValues.Any())
            {
                sessionId = headerSessionIdValues.First();
            }
            if (string.IsNullOrWhiteSpace(sessionId) && IsSessionIdCreationEnabled)
            {
                sessionId = _sessionIdProvider.SessionId(_hubAnalyticsClient.ClientConfiguration, application);
            }
            
            return sessionId;
        }

        private string GetUserId(HttpApplication application)
        {
            string userId = null;
            string[] headerUserIdValues = application.Request.Headers.GetValues(UserIdKey);
            if (headerUserIdValues != null && headerUserIdValues.Any())
            {
                userId = headerUserIdValues.First();
            }
            if (string.IsNullOrWhiteSpace(userId) && IsUserIdCreationEnabled)
            {
                userId = _userIdProvider.UserId(_hubAnalyticsClient.ClientConfiguration, application);
            }

            return userId;
        }

        // We do this dynamically as many of these are dynamic with more heading that way
        private string UserIdKey => _hubAnalyticsClient.ClientConfiguration.UserIdKey;
        private string SessionIdKey => _hubAnalyticsClient.ClientConfiguration.SessionIdKey;
        private string StopwatchKey => _hubAnalyticsClient.ClientConfiguration.HttpStopwatchKey;
        private string CorrelationIdKey => _hubAnalyticsClient.ClientConfiguration.CorrelationIdKey;
        private bool IsCaptureHttpEnabled => _hubAnalyticsClient.ClientConfiguration.IsCaptureHttpEnabled;
        private IReadOnlyCollection<string> CapturedRequestHeaders => _hubAnalyticsClient.ClientConfiguration.HttpRequestHeaderWhitelist;
        private IReadOnlyCollection<string> CapturedResponseHeaders => _hubAnalyticsClient.ClientConfiguration.HttpResponseHeaderWhitelist;
        private bool StripQueryParams => _hubAnalyticsClient.ClientConfiguration.StripHttpQueryParams;
        public bool IsSessionIdCreationEnabled => _hubAnalyticsClient.ClientConfiguration.IsSessionIdCreationEnabled;
        public bool IsUserIdCreationEnabled => _hubAnalyticsClient.ClientConfiguration.IsUserIdCreationEnabled;
    }
}
