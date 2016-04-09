using System.Collections.Generic;
using System.Web.Mvc;
using HubAnalytics.Core;

namespace HubAnalytics.MVC5
{
    public class ExceptionCaptureFilter : HandleErrorAttribute
    {
        private readonly IMicroserviceAnalyticClient _microserviceAnalyticClient;
        
        public ExceptionCaptureFilter(MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory = null)
        {
            if (microserviceAnalyticClientFactory == null)
            {
                microserviceAnalyticClientFactory = new MicroserviceAnalyticClientFactory();
            }

            _microserviceAnalyticClient = microserviceAnalyticClientFactory.GetClient();
        }

        public override void OnException(ExceptionContext filterContext)
        {
            Dictionary<string, string> additionalData = new Dictionary<string, string>
            {
                { "action", filterContext.RouteData.Values["action"].ToString() },
                {"controller", filterContext.RouteData.Values["controller"].ToString() }
            };

            _microserviceAnalyticClient.Error(filterContext.Exception, additionalData);
        }
    }
}
