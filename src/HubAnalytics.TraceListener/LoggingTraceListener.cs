using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using HubAnalytics.Core;

namespace HubAnalytics.TraceListener
{
    /// <summary>
    /// Event level for logging. Can be used as a category when tracing and it will result in logger output with a level
    /// </summary>
    public enum LogEventLevelEnum
    {
        Verbose = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5
    };

    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class LoggingTraceListener : System.Diagnostics.TraceListener
    {
        private readonly Regex _levelMatch = new Regex(@"^(\w+)+[:]");

        private readonly IMicroserviceAnalyticClient _microserviceAnalyticClient;

        private readonly ReadOnlyDictionary<string, LogEventLevelEnum> _mapping;

        public LoggingTraceListener() : this(null)
        {
            
        }        

        public LoggingTraceListener(IMicroserviceAnalyticClientFactory microserviceAnalyticClientFactory)
        {
            if (microserviceAnalyticClientFactory == null)
            {
                microserviceAnalyticClientFactory = new MicroserviceAnalyticClientFactory();
            }
            _microserviceAnalyticClient = microserviceAnalyticClientFactory.GetClient();

            _mapping = new ReadOnlyDictionary<string, LogEventLevelEnum>(
                Enum.GetValues(typeof(LogEventLevelEnum))
                    .OfType<LogEventLevelEnum>()
                    .ToDictionary(x => x.ToString().ToLower(), x => x));
        }

        public override void Write(string message)
        {
            Log(message);
        }

        public override void WriteLine(string message)
        {
            Log(message);
        }

        private void Log(string message)
        {
            Match match = _levelMatch.Match(message);
            if (!string.IsNullOrWhiteSpace(match.Value))
            {
                LogEventLevelEnum level;
                if (_mapping.TryGetValue(match.Groups[1].Value.ToLower(), out level))
                {
                    string trimmedMessage = message.Substring(match.Value.Length).TrimStart();
                    _microserviceAnalyticClient.Log(trimmedMessage, (int) level, level.ToString(), DateTimeOffset.UtcNow, null, null);
                    return;
                }
            }
            _microserviceAnalyticClient.Log(message, (int) LogEventLevelEnum.Information,
                LogEventLevelEnum.Information.ToString(), DateTimeOffset.UtcNow, null, null);
        }
    }
}
