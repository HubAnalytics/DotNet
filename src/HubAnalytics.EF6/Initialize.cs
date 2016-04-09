using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using HubAnalytics.Core;
using HubAnalytics.EF6.Proxies;

namespace HubAnalytics.EF6
{
    public static class MicroserviceAnalytics
    {
        private static bool _isInitialized;

        public static void Attach()
        {
            Attach(new MicroserviceAnalyticClientFactory());
        }

        public static void Attach(MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory)
        {
            if (_isInitialized) return;

            IMicroserviceAnalyticClient microserviceAnalyticClient = microserviceAnalyticClientFactory.GetClient();

            DbConfiguration.Loaded += (_, a) =>
            {
                a.ReplaceService<DbProviderServices>((s, k) => s.GetType() == typeof (ProxyDbProviderServices) ? s : new ProxyDbProviderServices(s, microserviceAnalyticClient));
                a.ReplaceService<IDbConnectionFactory>((s, k) => new ProxyDbConnectionFactory(s, microserviceAnalyticClient));
            };

            DbConfiguration.Loaded += (_, a) =>
                a.AddDependencyResolver(new InvariantNameResolver(), true);

            _isInitialized = true;
        }
    }
}
