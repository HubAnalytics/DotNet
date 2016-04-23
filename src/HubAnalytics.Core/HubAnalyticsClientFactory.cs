using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HubAnalytics.Core.Helpers;
using HubAnalytics.Core.Implementation;

namespace HubAnalytics.Core
{
    public class HubAnalyticsClientFactory : IHubAnalyticsClientFactory
    {
        private static readonly object ClientLockObject = new object();
        private static readonly object ClientConfigurationLockObject = new object();

        private static IHubAnalyticsClient _hubAnalyticsClient;
        private static IReadOnlyCollection<IDataCapturePlugin> _dataCapturePlugins;
        private static IClientConfiguration _clientConfiguration;        

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
                            GetJsonSerialization(),
                            clientConfiguration);
                        IReadOnlyCollection<Type> loadedPluginTypes = GetInterfaceImplementationLocator().Implements<IDataCapturePlugin>();
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
                        _clientConfiguration = GetClientConfigurationProvider().Get();
                        if (_clientConfiguration.IsRemoteUpdateEnabled)
                        {
                            _clientConfiguration = new RemoteClientConfiguration(_clientConfiguration, GetJsonSerialization());
                        }
                    }
                }
            }
            return _clientConfiguration;
        }

        public virtual IRuntimeProviderDiscoveryService GetRuntimeProviderDiscoveryService()
        {
            return new DefaultRuntimeProviderDiscoveryService(GetInterfaceImplementationLocator());
        }

        public virtual IInterfaceImplementationLocator GetInterfaceImplementationLocator()
        {
            return new InterfaceImplementationLocator();
        }

        public virtual IContextualIdProvider GetCorrelationIdProvider()
        {
#if DNXCORE50
            return new CallContextContextualIdProvider();
#else
            return new CallContextContextualIdProvider(GetClientConfiguration().CorrelationIdKey);
#endif
        }

        public virtual IClientConfigurationProvider GetClientConfigurationProvider()
        {
            return new PlatformDefaultConfigurationProvider();
        }

        public virtual IEnvironmentCapture GetEnvironmentCapture()
        {
            return new EnvironmentCapture();
        }

        public virtual IStackTraceParser GetStackTraceParser()
        {
            return new StackTraceParser();
        }

        public virtual IJsonSerialization GetJsonSerialization()
        {
            return new DefaultJsonSerialization();
        }

        public virtual IReadOnlyCollection<IDataCapturePlugin> DataCapturePlugins => _dataCapturePlugins;
    }
}
