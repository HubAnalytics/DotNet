using System.Web.Mvc;
using HubAnalytics.Core;

namespace HubAnalytics.MVC5
{
    public static class GlobalFilterCollectionExtensions
    {
        public static GlobalFilterCollection EnableErrorCapture(this GlobalFilterCollection filters, MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory = null)
        {
            filters.Add(new ExceptionCaptureFilter(microserviceAnalyticClientFactory));
            return filters;
        }

        public static GlobalFilterCollection EnableCorrelation(this GlobalFilterCollection filters, MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory = null)
        {
            filters.Add(new HttpCorrelatorAttribute(microserviceAnalyticClientFactory));
            return filters;
        }
    }
}
