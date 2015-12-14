using System.IO;
using System.Text;
using MicroserviceAnalytics.Core;
using Serilog.Core;
using Serilog.Events;

namespace MicroserviceAnalytics.Serilog
{
    public class MicroserviceAnalyticsSink : ILogEventSink
    {
        private readonly IMicroserviceAnalyticClient _client;
        private readonly JsonPropertyFormatter _formatter = new JsonPropertyFormatter();

        public MicroserviceAnalyticsSink(IMicroserviceAnalyticClientFactory factory = null)
        {
            if (factory == null)
            {
                factory = new MicroserviceAnalyticClientFactory();
            }

            _client = factory.GetClient();
        }

        public void Emit(LogEvent logEvent)
        {
            string message = logEvent.RenderMessage();
            int levelRank = (int)logEvent.Level;
            string levelText = logEvent.Level.ToString();
            string payloadJsonString = null;

            if (logEvent.Properties != null && logEvent.Properties.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                StringWriter writer = new StringWriter(sb);
                //_formatter.Format(logEvent, writer);
                _formatter.WriteProperties(logEvent.Properties, writer);
                payloadJsonString = sb.ToString();
            }
            
            _client.LogWithJson(message, levelRank, levelText, logEvent.Timestamp, logEvent.Exception, payloadJsonString);
        }
    }
}
