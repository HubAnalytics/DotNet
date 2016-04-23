using HubAnalytics.Core.Model;

#if! DNXCORE50
using Microsoft.VisualBasic.Devices;
#endif

namespace HubAnalytics.Core.Helpers
{
#if !DNXCORE50
    internal class EnvironmentCapture : IEnvironmentCapture
    {
        public Environment Get()
        {
            ComputerInfo info = new ComputerInfo();
            
            return new Environment
            {
                AvailablePhysicalMemory = info.AvailablePhysicalMemory,
                Locale = info.InstalledUICulture.Name,
                MachineName = System.Environment.MachineName,
                OperatingSystemVerson = System.Environment.OSVersion.VersionString,
                ProcessorCount = System.Environment.ProcessorCount,
                TotalPhysicalMemory = info.TotalPhysicalMemory
            };
        }
    }
#else
    internal class EnvironmentCapture : IEnvironmentCapture
    {
        public Environment Get()
        {
            return null;
        }
    }
#endif
}
