using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using HubAnalytics.Core;
using HubAnalytics.EF6.Proxies;

namespace HubAnalytics.EF6
{
    public static class HubAnalytics
    {
        private static bool _isInitialized;

        public static void Attach()
        {
            Attach(new HubAnalyticsClientFactory());
        }

        public static void Attach(IHubAnalyticsClientFactory hubAnalyticsClientFactory)
        {
            if (_isInitialized) return;

            IHubAnalyticsClient hubAnalyticsClient = hubAnalyticsClientFactory.GetClient();

            DbConfiguration.Loaded += (_, a) =>
            {
                a.ReplaceService<DbProviderServices>((s, k) => s.GetType() == typeof (ProxyDbProviderServices) ? s : new ProxyDbProviderServices(s, hubAnalyticsClient));
                a.ReplaceService<IDbConnectionFactory>((s, k) => new ProxyDbConnectionFactory(s, hubAnalyticsClient));
            };

            DbConfiguration.Loaded += (_, a) =>
                a.AddDependencyResolver(new InvariantNameResolver(), true);

            _isInitialized = true;
        }
    }
}
