using HubAnalytics.Core.Implementation;

namespace HubAnalytics.Core
{
    public class MicroserviceAnalyticClientFactory : IMicroserviceAnalyticClientFactory
    {
        private readonly IRuntimeProviderDiscoveryService _runtimeProviderDiscoveryService;
        private readonly IClientConfigurationProvider _clientConfigurationProvider;
        private static readonly object ClientLockObject = new object();
        private static readonly object ClientConfigurationLockObject = new object();
        private static IMicroserviceAnalyticClient _microserviceAnalyticClient;
        private static IClientConfiguration _clientConfiguration;

        public MicroserviceAnalyticClientFactory(IClientConfigurationProvider clientConfigurationProvider = null, IRuntimeProviderDiscoveryService runtimeProviderDiscoveryService=null)
        {
            _runtimeProviderDiscoveryService = runtimeProviderDiscoveryService ?? new DefaultRuntimeProviderDiscoveryService();
            _clientConfigurationProvider = clientConfigurationProvider ?? new PlatformDefaultConfigurationProvider();
        }

        public IMicroserviceAnalyticClient GetClient()
        {
            if (_microserviceAnalyticClient == null)
            {
                lock (ClientLockObject)
                {
                    if (_microserviceAnalyticClient == null)
                    {
                        IClientConfiguration clientConfiguration = GetClientConfiguration();
                        _microserviceAnalyticClient = new MicroserviceAnalyticClient(
                            clientConfiguration.PropertyId,
                            clientConfiguration.Key,
                            GetEnvironmentCapture(),
                            GetCorrelationIdProvider(),
                            GetStackTraceParser(),
                            clientConfiguration);
                    }
                }
            }

            return _microserviceAnalyticClient;
        }

        public virtual IClientConfiguration GetClientConfiguration()
        {
            if (_clientConfiguration == null)
            {
                lock (ClientConfigurationLockObject)
                {
                    if (_clientConfiguration == null)
                    {
                        _clientConfiguration = _clientConfigurationProvider.Get();
                        if (_clientConfiguration.IsRemoteUpdateEnabled)
                        {
                            _clientConfiguration = new RemoteClientConfiguration(_clientConfiguration);
                        }
                    }
                }
            }
            return _clientConfiguration;
        }

        public virtual IContextualIdProvider GetCorrelationIdProvider()
        {
#if DNXCORE50
            return new CallContextContextualIdProvider();
#else
            return new CallContextContextualIdProvider(GetClientConfiguration().CorrelationIdKey);
#endif
        }

        public virtual IEnvironmentCapture GetEnvironmentCapture()
        {
            return new EnvironmentCapture();
        }

        public virtual IStackTraceParser GetStackTraceParser()
        {
            return new StackTraceParser();
        }

        public virtual IRuntimeProviderDiscoveryService GetRuntimeProviderDiscoveryService()
        {
            return _runtimeProviderDiscoveryService;
        }        
    }
}
