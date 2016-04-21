using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HubAnalytics.Core.Helpers;
using HubAnalytics.Core.Implementation;

namespace HubAnalytics.Core
{
    public class HubAnalyticsClientFactory : IHubAnalyticsClientFactory
    {
        private readonly IRuntimeProviderDiscoveryService _runtimeProviderDiscoveryService;
        private readonly IClientConfigurationProvider _clientConfigurationProvider;
        private readonly IInterfaceImplementationLocator _interfaceImplementationLocator;
        private static readonly object ClientLockObject = new object();
        private static readonly object ClientConfigurationLockObject = new object();
        private static IHubAnalyticsClient _hubAnalyticsClient;
        private static IReadOnlyCollection<IDataCapturePlugin> _dataCapturePlugins;
        private static IClientConfiguration _clientConfiguration;        

        public HubAnalyticsClientFactory(IClientConfigurationProvider clientConfigurationProvider = null, IRuntimeProviderDiscoveryService runtimeProviderDiscoveryService=null, IInterfaceImplementationLocator interfaceImplementationLocator = null)
        {
            _interfaceImplementationLocator = interfaceImplementationLocator ?? new InterfaceImplementationLocator();
            _runtimeProviderDiscoveryService = runtimeProviderDiscoveryService ?? new DefaultRuntimeProviderDiscoveryService(_interfaceImplementationLocator);
            _clientConfigurationProvider = clientConfigurationProvider ?? new PlatformDefaultConfigurationProvider();
        }

        public IHubAnalyticsClient GetClient()
        {
            if (_hubAnalyticsClient == null)
            {
                lock (ClientLockObject)
                {
                    if (_hubAnalyticsClient == null)
                    {
                        IClientConfiguration clientConfiguration = GetClientConfiguration();
                        _hubAnalyticsClient = new HubAnalyticsClient(
                            clientConfiguration.PropertyId,
                            clientConfiguration.Key,
                            GetEnvironmentCapture(),
                            GetCorrelationIdProvider(),
                            GetStackTraceParser(),
                            clientConfiguration);
                        IReadOnlyCollection<Type> loadedPluginTypes = _interfaceImplementationLocator.Implements<IDataCapturePlugin>();
                        List<IDataCapturePlugin> plugins = new List<IDataCapturePlugin>(loadedPluginTypes.Count);
                        foreach (Type pluginType in loadedPluginTypes)
                        {
                            IDataCapturePlugin plugin = (IDataCapturePlugin)Activator.CreateInstance(pluginType);
                            plugin.Initialize(_hubAnalyticsClient);
                            plugins.Add(plugin);
                        }
                        _dataCapturePlugins = new ReadOnlyCollection<IDataCapturePlugin>(plugins);
                    }
                }

            }

            return _hubAnalyticsClient;
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

        public virtual IReadOnlyCollection<IDataCapturePlugin> DataCapturePlugins => _dataCapturePlugins;
    }
}
