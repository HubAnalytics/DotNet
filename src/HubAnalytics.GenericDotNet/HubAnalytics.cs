using System;
using HubAnalytics.Core;

namespace HubAnalytics.GenericDotNet
{
    public static class HubAnalytics
    {
        public static void Attach(IHubAnalyticsClientFactory hubAnalyticsClientFactory = null)
        {
            if (hubAnalyticsClientFactory == null)
            {
                hubAnalyticsClientFactory = new HubAnalyticsClientFactory();
            }

            IHubAnalyticsClient recorder = hubAnalyticsClientFactory.GetClient();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = args.ExceptionObject as Exception;
                if (ex != null)
                {
                    recorder.Error(ex);
                }
            };
        }
    }
}
