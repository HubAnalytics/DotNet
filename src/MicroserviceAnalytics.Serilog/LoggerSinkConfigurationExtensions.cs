using MicroserviceAnalytics.Core;
using Serilog;
using Serilog.Configuration;

namespace MicroserviceAnalytics.Serilog
{
    public static class LoggerSinkConfigurationExtensions
    {
        public static LoggerConfiguration MicroserviceAnalytics(this LoggerSinkConfiguration sinkConfiguration, IMicroserviceAnalyticClientFactory factory = null)
        {
            MicroserviceAnalyticsSink sink = new MicroserviceAnalyticsSink(factory);
            return sinkConfiguration.Sink(sink);
        }
    }
}
