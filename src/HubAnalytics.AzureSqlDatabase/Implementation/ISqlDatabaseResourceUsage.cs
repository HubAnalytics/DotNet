using System.Collections.Generic;
using System.Threading.Tasks;
using HubAnalytics.Core.Model;

namespace HubAnalytics.AzureSqlDatabase.Implementation
{
    internal interface ISqlDatabaseResourceUsage
    {
        string Name { get; }
        int ConcurrentFailures { get; }
        Task<bool> Update();
        IReadOnlyCollection<Event> GetEvents();
    }
}