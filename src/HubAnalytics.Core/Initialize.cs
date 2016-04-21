using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubAnalytics.Core
{
    public static class Initialize
    {
        public static void Start(IHubAnalyticsClientFactory hubAnalyticsClientFactory = null)
        {
            hubAnalyticsClientFactory = hubAnalyticsClientFactory ?? new HubAnalyticsClientFactory();
            hubAnalyticsClientFactory.GetClient();
        }
    }
}
