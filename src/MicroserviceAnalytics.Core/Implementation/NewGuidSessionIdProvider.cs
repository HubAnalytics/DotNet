using System;

namespace MicroserviceAnalytics.Core.Implementation
{
    public class NewGuidSessionIdProvider : ISessionIdProvider
    {
        public string SessionId(IClientConfiguration configuration, object context)
        {
            return Guid.NewGuid().ToString();
        }
    }
}
