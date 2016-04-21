using Microsoft.Web.Infrastructure.DynamicModuleHelper;

namespace HubAnalytics.AspNet4
{
    public static class Initialize
    {
        public static void LoadModule()
        {
            DynamicModuleUtility.RegisterModule(typeof(DataCollectionHttpModule));
        }
    }
}
