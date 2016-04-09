namespace HubAnalytics.Core.Model
{
    public class StackTraceEntry
    {
        public int Line { get; set; }

        public int Column { get; set; }

        public string Class { get; set; }

        public string Filename { get; set; }

        public string Method { get; set; }

        public string Assembly { get; set; }
    }
}
