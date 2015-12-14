using System;
using System.Collections.Generic;

namespace MicroserviceAnalytics.Core.Model
{
    public class EventBatch
    {
        public string ApplicationVersion { get; set; }

        public Environment Environment { get; set; }

        public List<Event> Events { get; set; } 

        public string Source { get; set; }

        public DateTimeOffset ReceivedAt { get; set; }
    }
}
