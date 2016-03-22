using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web;
using MicroserviceAnalytics.AspNet4.Implementation;
using MicroserviceAnalytics.Core;

namespace MicroserviceAnalytics.AspNet4
{
    public class DataCollectionHttpModule : IHttpModule
    {
        private readonly IHeaderParser _headerParser;
        private readonly IMicroserviceAnalyticClient _microserviceAnalyticClient;
        private readonly IContextualIdProvider _contextualIdProvider;
        private readonly IUserIdProvider _userIdProvider;
        private readonly ISessionIdProvider _sessionIdProvider;
        
        public DataCollectionHttpModule() : this(null, null)
        {
            
        }

        public DataCollectionHttpModule(IMicroserviceAnalyticClientFactory microserviceAnalyticClientFactory, IHeaderParser headerParser)
        {
            if (microserviceAnalyticClientFactory == null)
            {
                microserviceAnalyticClientFactory = new MicroserviceAnalyticClientFactory();
            }
            if (headerParser == null)
            {
                headerParser = new HeaderParser();
            }
            IClientConfiguration clientConfiguration = microserviceAnalyticClientFactory.GetClientConfiguration();
            _headerParser = headerParser;
            _microserviceAnalyticClient = microserviceAnalyticClientFactory.GetClient();
            _contextualIdProvider = microserviceAnalyticClientFactory.GetCorrelationIdProvider();
            _sessionIdProvider = clientConfiguration.IsSessionIdCreationEnabled ? microserviceAnalyticClientFactory.GetRuntimeProviderDiscoveryService().SessionIdProvider : null;
            _userIdProvider = clientConfiguration.IsSessionIdCreationEnabled ? microserviceAnalyticClientFactory.GetRuntimeProviderDiscoveryService().UserIdProvider : null;
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
                _microserviceAnalyticClient.Error(ex);
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
                _microserviceAnalyticClient.Error(ex);
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
                _microserviceAnalyticClient.Error(ex);
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
                    _microserviceAnalyticClient.Error(ex);
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
            string userId = GetUserId(request);
            string sessionId = GetSessionId(request);
            Dictionary<string, string[]> requestHeaders = _headerParser.CaptureHeaders(CapturedRequestHeaders, request.Headers);
            Dictionary<string, string[]> responseHeaders = _headerParser.CaptureHeaders(CapturedResponseHeaders, HttpContext.Current.Response.Headers);

            _microserviceAnalyticClient.HttpRequest(verb,
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

            if (_microserviceAnalyticClient.ClientConfiguration.EnableCorrelation)
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

            if (_microserviceAnalyticClient.ClientConfiguration.IsSessionTrackingEnabled)
            {
                values = application.Request.Headers.GetValues(SessionIdKey);
                if (values != null && values.Any())
                {
                    sessionId = values[0];
                }
                _contextualIdProvider.SessionId = sessionId;
            }

            if (_microserviceAnalyticClient.ClientConfiguration.IsUserTrackingEnabled)
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
            if (!_microserviceAnalyticClient.ClientConfiguration.EnableCorrelation) return;

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
                HttpRequest request = application.Request;
                var sessionId = GetSessionId(request);
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    application.Response.Headers.Add(SessionIdKey, sessionId);
                }
            }
            responseValues = application.Response.Headers.GetValues(UserIdKey);
            if (responseValues == null || !responseValues.Any())
            {
                HttpRequest request = application.Request;
                var userId = GetUserId(request);
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

        private string GetSessionId(HttpRequest request)
        {
            string sessionId = null;
            string[] headerSessionIdValues = request.Headers.GetValues(SessionIdKey);
            if (headerSessionIdValues != null && headerSessionIdValues.Any())
            {
                sessionId = headerSessionIdValues.First();
            }
            
            return sessionId;
        }

        private string GetUserId(HttpRequest request)
        {
            string userId = null;
            string[] headerUserIdValues = request.Headers.GetValues(UserIdKey);
            if (headerUserIdValues != null && headerUserIdValues.Any())
            {
                userId = headerUserIdValues.First();
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                if (IsUserIdCreationEnabled)
                {
                    _userIdProvider.UserId(_microserviceAnalyticClient.ClientConfiguration, request);
                }

                
            }

            return userId;
        }

        // We do this dynamically as many of these are dynamic with more heading that way
        private string UserIdKey => _microserviceAnalyticClient.ClientConfiguration.UserIdKey;
        private string SessionIdKey => _microserviceAnalyticClient.ClientConfiguration.SessionIdKey;
        private string StopwatchKey => _microserviceAnalyticClient.ClientConfiguration.HttpStopwatchKey;
        private string CorrelationIdKey => _microserviceAnalyticClient.ClientConfiguration.CorrelationIdKey;
        private bool IsCaptureHttpEnabled => _microserviceAnalyticClient.ClientConfiguration.IsCaptureHttpEnabled;
        private IReadOnlyCollection<string> CapturedRequestHeaders => _microserviceAnalyticClient.ClientConfiguration.HttpRequestHeaderWhitelist;
        private IReadOnlyCollection<string> CapturedResponseHeaders => _microserviceAnalyticClient.ClientConfiguration.HttpResponseHeaderWhitelist;
        private bool StripQueryParams => _microserviceAnalyticClient.ClientConfiguration.StripHttpQueryParams;
        private string TrackingCookieName => _microserviceAnalyticClient.ClientConfiguration.TrackingCookieName;
        public bool IsSessionIdCreationEnabled => _microserviceAnalyticClient.ClientConfiguration.IsSessionIdCreationEnabled;
        public bool IsUserIdCreationEnabled => _microserviceAnalyticClient.ClientConfiguration.IsUserIdCreationEnabled;
    }
}
