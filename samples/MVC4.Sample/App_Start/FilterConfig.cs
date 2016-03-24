using System.Web;
using System.Web.Mvc;
using MicroserviceAnalytics.MVC5;

namespace MVC4.Sample
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.EnableCorrelation().EnableErrorCapture();
            filters.Add(new HandleErrorAttribute());
        }
    }
}
