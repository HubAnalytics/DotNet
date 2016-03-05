using System.Collections.Generic;

namespace MicroserviceAnalytics.Core.Model
{
    public class Event
    {
        public const string EventDateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

        public string EventType { get; set; }

        public string EventStartDateTime { get; set; }

        public string EventEndDateTime { get; set; }

        public List<string> CorrelationIds { get; set; }

        public string SessionId { get; set; }

        public string UserId { get; set; }

        public List<int> CorrelationDepths { get; set; } 

        public Dictionary<string, object> Data { get; set; } 
    }
}
