using System.Data.Common;
using HubAnalytics.Ado.Proxies;
using HubAnalytics.Core;
using DbCommandDefinition = System.Data.Entity.Core.Common.DbCommandDefinition;

namespace HubAnalytics.EF6.Proxies
{
    public class ProxyDbCommandDefinition : DbCommandDefinition
    {
        private readonly DbCommandDefinition _proxiedCommandDefinition;
        private readonly IHubAnalyticsClient _hubAnalyticsClient;

        public ProxyDbCommandDefinition(DbCommandDefinition proxiedCommandDefinition, IHubAnalyticsClient hubAnalyticsClient)
        {
            _proxiedCommandDefinition = proxiedCommandDefinition;
            _hubAnalyticsClient = hubAnalyticsClient;
        }

        public override DbCommand CreateCommand()
        {
            return new ProxyDbCommand(_proxiedCommandDefinition.CreateCommand(), _hubAnalyticsClient);
        }
    }
}
