using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web;
using MicroserviceAnalytics.Core;

namespace MicroserviceAnalytics.AspNet4
{
    public class DataCollectionHttpModule : IHttpModule
    {
        private readonly IMicroserviceAnalyticClient _microserviceAnalyticClient;
        private readonly string _httpCorrelationHeaderKey;
        private readonly ICorrelationIdProvider _correlationIdProvider;
        private readonly bool _correlationIdEnabled;
        private readonly bool _httpTracingStripQueryParams;
        private readonly IReadOnlyCollection<string> _httpCaptureRequestHeaders;
        private readonly IReadOnlyCollection<string> _httpCaptureResponseHeaders;
        private readonly bool _httpTracingEnabled;
        private readonly string _stopwatchKey;

        public DataCollectionHttpModule() : this(null)
        {
            
        }

        public DataCollectionHttpModule(MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory)
        {
            if (microserviceAnalyticClientFactory == null)
            {
                microserviceAnalyticClientFactory = new MicroserviceAnalyticClientFactory();
            }
            IClientConfiguration clientConfiguration = microserviceAnalyticClientFactory.GetClientConfiguration();
            _microserviceAnalyticClient = microserviceAnalyticClientFactory.GetClient();

            _httpCorrelationHeaderKey = clientConfiguration.CorrelationIdKey;
            _correlationIdEnabled = clientConfiguration.EnableCorrelation;
            _correlationIdProvider = microserviceAnalyticClientFactory.GetCorrelationIdProvider();

            _httpTracingEnabled = clientConfiguration.IsCaptureHttpEnabled;
            _httpCaptureRequestHeaders = clientConfiguration.HttpRequestHeaderWhitelist;
            _httpCaptureResponseHeaders = clientConfiguration.HttpResponseHeaderWhitelist;
            _stopwatchKey = clientConfiguration.HttpStopwatchKey;
            _httpTracingStripQueryParams = clientConfiguration.StripHttpQueryParams;
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
                if (_correlationIdEnabled)
                {
                    SetCorrelationIdOnResponse(sender);
                }
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
                if (_httpTracingEnabled)
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
                if (_correlationIdEnabled)
                {
                    SetCorrelationIdOnRequest(sender);
                }
                if (_httpTracingEnabled)
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
            application.Context.Items[_stopwatchKey] = sw;
            sw.Start();
        }

        private void EndHttpTrace(object sender)
        {
            HttpApplication application = (HttpApplication)sender;
            
            long elapsedMilliseconds = 0;
            Stopwatch sw = (Stopwatch) application.Context.Items[_stopwatchKey];
            if (sw != null)
            {
                sw.Stop();
                elapsedMilliseconds = sw.ElapsedMilliseconds;
            }
            
            HttpRequest request = application.Request;
            
            string uri = request.Url.ToString();
            bool didStripQueryParams = false;
            if (_httpTracingStripQueryParams && request.QueryString.Count > 0)
            {
                uri = uri.Substring(0, uri.IndexOf("?", StringComparison.Ordinal));
                didStripQueryParams = true;
            }
            string verb = request.HttpMethod;
            string correlationId = GetCorrelationId(request);
            Dictionary<string, string[]> requestHeaders = CaptureHeaders(_httpCaptureRequestHeaders, request.Headers);
            Dictionary<string, string[]> responseHeaders = CaptureHeaders(_httpCaptureResponseHeaders, HttpContext.Current.Response.Headers);

            _microserviceAnalyticClient.HttpRequest(verb,
                HttpContext.Current.Response.StatusCode,
                uri,
                didStripQueryParams,
                correlationId,
                DateTimeOffset.UtcNow.AddMilliseconds(-elapsedMilliseconds),
                elapsedMilliseconds,
                requestHeaders,
                responseHeaders);
        }

        private void SetCorrelationIdOnRequest(object sender)
        {
            HttpApplication application = (HttpApplication)sender;
            string correlationId;

            string[] values = application.Request.Headers.GetValues(_httpCorrelationHeaderKey);
            if (values != null && values.Any())
            {
                correlationId = values[0];
            }
            else
            {
                correlationId = Guid.NewGuid().ToString();
                application.Request.Headers.Add(_httpCorrelationHeaderKey, correlationId);
            }

            _correlationIdProvider.CorrelationId = correlationId;
        }

        private void SetCorrelationIdOnResponse(object sender)
        {
            HttpApplication application = (HttpApplication)sender;

            string[] responseValues = application.Response.Headers.GetValues(_httpCorrelationHeaderKey);
            if (responseValues == null || !responseValues.Any())
            {
                HttpRequest request = application.Request;
                var correlationId = GetCorrelationId(request);

                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    application.Response.Headers.Add(_httpCorrelationHeaderKey, correlationId);
                }
            }
        }

        private string GetCorrelationId(HttpRequest request)
        {
            string correlationId;
            string[] headerCorrelationIdValues = request.Headers.GetValues(_httpCorrelationHeaderKey);
            if (headerCorrelationIdValues != null && headerCorrelationIdValues.Any())
            {
                correlationId = headerCorrelationIdValues.First();
            }
            else
            {
                correlationId = _correlationIdProvider.CorrelationId;
            }
            return correlationId;
        }

        private Dictionary<string, string[]> CaptureHeaders(IReadOnlyCollection<string> captureHeaders, NameValueCollection headers)
        {
            Dictionary<string, string[]> capturedHeaders = null;
            if (captureHeaders != null && captureHeaders.Any())
            {
                capturedHeaders = new Dictionary<string, string[]>();
                if (captureHeaders.First() == "*")
                {
                    foreach (string key in headers.AllKeys)
                    {
                        string[] values = headers.GetValues(key);
                        capturedHeaders[key] = values;
                    }
                }
                else
                {
                    foreach (string headerName in captureHeaders)
                    {
                        string[] values = headers.GetValues(headerName);
                        if (values != null && values.Any())
                        {
                            capturedHeaders.Add(headerName, values);
                        }
                    }
                }
            }
            return capturedHeaders;
        }
    }
}
