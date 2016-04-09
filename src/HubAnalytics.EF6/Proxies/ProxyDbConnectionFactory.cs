using System.Data.Common;
using System.Data.Entity.Infrastructure;
using HubAnalytics.Ado.Proxies;
using HubAnalytics.Core;

namespace HubAnalytics.EF6.Proxies
{
    public class ProxyDbConnectionFactory : IDbConnectionFactory
    {
        private readonly IDbConnectionFactory _proxiedFactory;
        private readonly IMicroserviceAnalyticClient _microserviceAnalyticClient;

        public ProxyDbConnectionFactory(IDbConnectionFactory proxiedFactory, IMicroserviceAnalyticClient microserviceAnalyticClient)
        {
            _proxiedFactory = proxiedFactory;
            _microserviceAnalyticClient = microserviceAnalyticClient;
        }

        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            DbConnection connection = _proxiedFactory.CreateConnection(nameOrConnectionString);
            ProxyDbConnection proxyConnection = connection as ProxyDbConnection;
            if (proxyConnection != null)
            {
                return proxyConnection;
            }
            return new ProxyDbConnection(connection, _microserviceAnalyticClient);
        }
    }
}
