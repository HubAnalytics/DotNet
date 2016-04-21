using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using HubAnalytics.Core;
using HubAnalytics.EF6.Proxies;

namespace HubAnalytics.EF6
{
    public class Ef6CapturePlugin : IDataCapturePlugin
    {
        private static bool _isInitialized;

        public void Initialize(IHubAnalyticsClient client)
        {
            if (_isInitialized) return;

            DbConfiguration.Loaded += (_, a) =>
            {
                a.ReplaceService<DbProviderServices>((s, k) => s.GetType() == typeof (ProxyDbProviderServices) ? s : new ProxyDbProviderServices(s, client));
                a.ReplaceService<IDbConnectionFactory>((s, k) => new ProxyDbConnectionFactory(s, client));
            };

            DbConfiguration.Loaded += (_, a) =>
                a.AddDependencyResolver(new InvariantNameResolver(), true);

            _isInitialized = true;
        }

        public static void Initialize(IHubAnalyticsClientFactory hubAnalyticsClientFactory = null)
        {
            hubAnalyticsClientFactory = hubAnalyticsClientFactory ?? new HubAnalyticsClientFactory();
            new Ef6CapturePlugin().Initialize(hubAnalyticsClientFactory.GetClient());
        }
    }
}
