using MicroserviceAnalytics.Core.Model;

namespace MicroserviceAnalytics.Core
{
    public interface IEnvironmentCapture
    {
        Environment Get();
    }
}
