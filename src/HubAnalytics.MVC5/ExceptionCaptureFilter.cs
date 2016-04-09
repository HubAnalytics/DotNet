using System.Collections.Generic;
using System.Web.Mvc;
using HubAnalytics.Core;

namespace HubAnalytics.MVC5
{
    public class ExceptionCaptureFilter : HandleErrorAttribute
    {
        private readonly IHubAnalyticsClient _hubAnalyticsClient;
        
        public ExceptionCaptureFilter(IHubAnalyticsClientFactory hubAnalyticsClientFactory = null)
        {
            if (hubAnalyticsClientFactory == null)
            {
                hubAnalyticsClientFactory = new HubAnalyticsClientFactory();
            }

            _hubAnalyticsClient = hubAnalyticsClientFactory.GetClient();
        }

        public override void OnException(ExceptionContext filterContext)
        {
            Dictionary<string, string> additionalData = new Dictionary<string, string>
            {
                { "action", filterContext.RouteData.Values["action"].ToString() },
                {"controller", filterContext.RouteData.Values["controller"].ToString() }
            };

            _hubAnalyticsClient.Error(filterContext.Exception, additionalData);
        }
    }
}
