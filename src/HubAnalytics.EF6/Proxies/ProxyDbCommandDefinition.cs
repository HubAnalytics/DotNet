using System.Data.Common;
using HubAnalytics.Ado.Proxies;
using HubAnalytics.Core;
using DbCommandDefinition = System.Data.Entity.Core.Common.DbCommandDefinition;

namespace HubAnalytics.EF6.Proxies
{
    public class ProxyDbCommandDefinition : DbCommandDefinition
    {
        private readonly DbCommandDefinition _proxiedCommandDefinition;
        private readonly IMicroserviceAnalyticClient _microserviceAnalyticClient;

        public ProxyDbCommandDefinition(DbCommandDefinition proxiedCommandDefinition, IMicroserviceAnalyticClient microserviceAnalyticClient)
        {
            _proxiedCommandDefinition = proxiedCommandDefinition;
            _microserviceAnalyticClient = microserviceAnalyticClient;
        }

        public override DbCommand CreateCommand()
        {
            return new ProxyDbCommand(_proxiedCommandDefinition.CreateCommand(), _microserviceAnalyticClient);
        }
    }
}
