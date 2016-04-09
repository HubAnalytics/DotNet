using System;

namespace HubAnalytics.Core.Implementation
{
    public class NewGuidUserIdProvider : IUserIdProvider
    {
        public string UserId(IClientConfiguration configuration, object context)
        {
            return Guid.NewGuid().ToString();
        }
    }
}
