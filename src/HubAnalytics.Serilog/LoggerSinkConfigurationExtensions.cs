using HubAnalytics.Core;
using Serilog;
using Serilog.Configuration;

namespace HubAnalytics.Serilog
{
    public static class LoggerSinkConfigurationExtensions
    {
        public static LoggerConfiguration HubAnalytics(this LoggerSinkConfiguration sinkConfiguration, IHubAnalyticsClientFactory factory = null)
        {
            HubAnalyticsSink sink = new HubAnalyticsSink(factory);
            return sinkConfiguration.Sink(sink);
        }
    }
}
