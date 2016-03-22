using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MicroserviceAnalytics.Core;

namespace MicroserviceAnalytics.MVC5
{
    public class MvcUserIdProvider : IUserIdProvider
    {
        public string UserId(IClientConfiguration configuration, object context)
        {
            string userId = null;
            HttpRequest request = (HttpRequest) context;
            HttpCookie trackingCookie = request.Cookies[configuration.TrackingCookieName];
            if (trackingCookie != null)
            {
                userId = trackingCookie.Values[configuration.UserIdKey];
            }
            if (string.IsNullOrWhiteSpace(userId) && configuration.IsUserIdCreationEnabled)
            {
                userId = Guid.NewGuid().ToString();
                // TODO: Do we need to set the value in the cookie here?
            }
            return userId;
        }
    }
}
