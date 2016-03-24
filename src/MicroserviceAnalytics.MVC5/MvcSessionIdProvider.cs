using System;
using System.Web;
using MicroserviceAnalytics.Core;

namespace MicroserviceAnalytics.MVC5
{
    public class MvcSessionIdProvider : ISessionIdProvider
    {
        public string SessionId(IClientConfiguration configuration, object context)
        {
            string sessionId = null;
            HttpApplication application = (HttpApplication)context;
            HttpRequest request = application.Request;
            HttpCookie trackingCookie = request.Cookies[configuration.TrackingSessionCookieName];
            if (trackingCookie != null)
            {
                sessionId = trackingCookie.Value;
            }
            if (string.IsNullOrWhiteSpace(sessionId) && configuration.IsSessionIdCreationEnabled)
            {
                sessionId = Guid.NewGuid().ToString();
                HttpCookie responseCookie = application.Response.Cookies[configuration.TrackingSessionCookieName];
                if (responseCookie == null)
                {
                    responseCookie = new HttpCookie(configuration.TrackingSessionCookieName);
                    application.Response.Cookies.Add(responseCookie);
                }
                responseCookie.Value = sessionId;
            }
            return sessionId;
        }
    }
}
