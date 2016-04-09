using System.Web.Mvc;
using HubAnalytics.Core;

namespace HubAnalytics.MVC5
{
    public static class GlobalFilterCollectionExtensions
    {
        public static GlobalFilterCollection EnableErrorCapture(this GlobalFilterCollection filters, HubAnalyticsClientFactory hubAnalyticsClientFactory = null)
        {
            filters.Add(new ExceptionCaptureFilter(hubAnalyticsClientFactory));
            return filters;
        }

        public static GlobalFilterCollection EnableCorrelation(this GlobalFilterCollection filters, HubAnalyticsClientFactory hubAnalyticsClientFactory = null)
        {
            filters.Add(new HttpCorrelatorAttribute(hubAnalyticsClientFactory));
            return filters;
        }
    }
}
