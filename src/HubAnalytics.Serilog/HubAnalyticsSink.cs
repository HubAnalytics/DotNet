using System.IO;
using System.Text;
using HubAnalytics.Core;
using Serilog.Core;
using Serilog.Events;

namespace HubAnalytics.Serilog
{
    public class HubAnalyticsSink : ILogEventSink
    {
        private readonly IHubAnalyticsClient _client;
        private readonly JsonPropertyFormatter _formatter = new JsonPropertyFormatter();

        public HubAnalyticsSink(IHubAnalyticsClientFactory factory = null)
        {
            if (factory == null)
            {
                factory = new HubAnalyticsClientFactory();
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
