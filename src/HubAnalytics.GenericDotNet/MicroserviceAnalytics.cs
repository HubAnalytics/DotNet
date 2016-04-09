using System;
using HubAnalytics.Core;

namespace HubAnalytics.GenericDotNet
{
    public static class MicroserviceAnalytics
    {
        public static void Attach(MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory = null)
        {
            if (microserviceAnalyticClientFactory == null)
            {
                microserviceAnalyticClientFactory = new MicroserviceAnalyticClientFactory();
            }

            IMicroserviceAnalyticClient recorder = microserviceAnalyticClientFactory.GetClient();
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
