using System.Collections.Generic;
using System.Threading.Tasks;

namespace HubAnalytics.AzureSqlDatabase.Implementation
{
    internal interface IUsageProvider
    {
        Task<IReadOnlyCollection<TelemetryItem>> Get(AzureSqlDatabase azureSqlDatabase);
    }
}
