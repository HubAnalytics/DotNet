namespace HubAnalytics.Core.Model
{
    public class Environment
    {
        public int ProcessorCount { get; set; }

        public string MachineName { get; set; }

        public string OperatingSystemVerson { get; set; }

        public string Locale { get; set; }

        public int UtcOffset { get; set; }

        public ulong TotalPhysicalMemory { get; set; }

        public ulong AvailablePhysicalMemory { get; set; }

        public string UserAgentString { get; set; }
    }
}
