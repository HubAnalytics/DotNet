using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using MicroserviceAnalytics.Core;
using MicroserviceAnalytics.EF6.Proxies;

namespace MicroserviceAnalytics.EF6
{
    public static class Initialize
    {
        private static bool _isInitialized;

        public static void StartDefaultFactory()
        {
            Start(new MicroserviceAnalyticClientFactory());
        }

        public static void Start(MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory)
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
