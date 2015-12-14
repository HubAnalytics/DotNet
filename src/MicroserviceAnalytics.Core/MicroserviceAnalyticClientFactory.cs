using MicroserviceAnalytics.Core.Implementation;

namespace MicroserviceAnalytics.Core
{
    public class MicroserviceAnalyticClientFactory : IMicroserviceAnalyticClientFactory
    {
        private readonly IClientConfigurationProvider _clientConfigurationProvider;
        private static readonly object ClientLockObject = new object();
        private static readonly object ClientConfigurationLockObject = new object();
        private static IMicroserviceAnalyticClient _microserviceAnalyticClient;
        private static IClientConfiguration _clientConfiguration;

        public MicroserviceAnalyticClientFactory(IClientConfigurationProvider clientConfigurationProvider = null)
        {
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

        public virtual ICorrelationIdProvider GetCorrelationIdProvider()
        {
#if DNXCORE50
            return new CallContextCorrelationIdProvider();
#else
            return new CallContextCorrelationIdProvider(GetClientConfiguration().CorrelationIdKey);
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
    }
}
