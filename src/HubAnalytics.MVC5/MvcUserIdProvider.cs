using System;
using System.Web;
using HubAnalytics.Core;

namespace HubAnalytics.MVC5
{
    public class MvcUserIdProvider : IUserIdProvider
    {
        public string UserId(IClientConfiguration configuration, object context)
        {
            string userId = null;
            HttpApplication application = (HttpApplication) context;
            HttpRequest request = application.Request;
            HttpCookie trackingCookie = request.Cookies[configuration.TrackingUserCookieName];
            if (trackingCookie != null)
            {
                userId = trackingCookie.Value;
            }
            if (string.IsNullOrWhiteSpace(userId) && configuration.IsUserIdCreationEnabled)
            {
                userId = Guid.NewGuid().ToString();
                HttpCookie responseCookie = application.Response.Cookies[configuration.TrackingUserCookieName];
                if (responseCookie == null)
                {
                    responseCookie = new HttpCookie(configuration.TrackingUserCookieName);
                    application.Response.Cookies.Add(responseCookie);                    
                }
                responseCookie.Expires = DateTime.Now.AddDays(30);
                responseCookie.Value = userId;
            }
            return userId;
        }
    }
}
