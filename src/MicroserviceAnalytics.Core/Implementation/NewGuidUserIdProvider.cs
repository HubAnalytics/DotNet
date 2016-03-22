using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroserviceAnalytics.Core.Implementation
{
    public class NewGuidUserIdProvider : IUserIdProvider
    {
        public string UserId(IClientConfiguration configuration, object context)
        {
            return Guid.NewGuid().ToString();
        }
    }
}
