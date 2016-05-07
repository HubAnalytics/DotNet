using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HubAnalytics.Core.Helpers;
using HubAnalytics.Core.HttpEventUrlParsers;
using HubAnalytics.Core.Implementation;

namespace HubAnalytics.Core
{
    public class HubAnalyticsClientFactory : IHubAnalyticsClientFactory
    {
        private static readonly object ClientLockObject = new object();
        private static readonly object ClientConfigurationLockObject = new object();

        private static IHubAnalyticsClient _hubAnalyticsClient;
        private static IClientConfiguration _clientConfiguration;
#if !DNXCORE50
        // ReSharper disable once NotAccessedField.Local
        private static HttpEventListener _eventListener;
#endif
        private static IReadOnlyCollection<IDataCapturePlugin> _dataCapturePlugins;
        

        public IHubAnalyticsClient GetClient()
        {
            if (_hubAnalyticsClient == null)
            {
                lock (ClientLockObject)
                {
                    if (_hubAnalyticsClient == null)
                    {
                        IUrlProcessor urlProcessor = null;
                        Type urlProcessorType = GetInterfaceImplementationLocator().Implements<IUrlProcessor>().FirstOrDefault();
                        if (urlProcessorType != null)
                        {
                            urlProcessor = (IUrlProcessor)Activator.CreateInstance(urlProcessorType);
                        }

                        IContextualIdProvider contextualIdProvider = GetCorrelationIdProvider();
                        IClientConfiguration clientConfiguration = GetClientConfiguration();
                        _hubAnalyticsClient = new HubAnalyticsClient(
                            clientConfiguration.PropertyId,
                            clientConfiguration.Key,
                            GetEnvironmentCapture(),
                            contextualIdProvider,
                            GetStackTraceParser(),
                            GetJsonSerialization(),
                            clientConfiguration,
                            urlProcessor);
                        IReadOnlyCollection<Type> loadedPluginTypes = GetInterfaceImplementationLocator().Implements<IDataCapturePlugin>();
                        List<IDataCapturePlugin> plugins = new List<IDataCapturePlugin>(loadedPluginTypes.Count);
                        foreach (Type pluginType in loadedPluginTypes)
                        {
                            IDataCapturePlugin plugin = (IDataCapturePlugin)Activator.CreateInstance(pluginType);
                            plugin.Initialize(_hubAnalyticsClient);
                            plugins.Add(plugin);
                        }
                        _dataCapturePlugins = new ReadOnlyCollection<IDataCapturePlugin>(plugins);

#if !DNXCORE50
                        IReadOnlyCollection<Type> httpEventUrlParserTypes = GetInterfaceImplementationLocator().Implements<IHttpEventUrlParser>();
                        IReadOnlyCollection<Type> userParsers = httpEventUrlParserTypes.Where(x => 
                            x != typeof(AzureQueueStorageParser) &&
                            x != typeof(AzureTableStorageParser) &&
                            x != typeof(AzureBlobStorageParser) &&
                            x != typeof(GenericParser)).ToList();
                        List<IHttpEventUrlParser> parsers = userParsers.Select(x => (IHttpEventUrlParser)Activator.CreateInstance(x)).ToList();
                        parsers.Add(new AzureQueueStorageParser());
                        parsers.Add(new AzureTableStorageParser());
                        parsers.Add(new AzureBlobStorageParser());
                        parsers.Add(new GenericParser());
                        _eventListener = new HttpEventListener(_hubAnalyticsClient, parsers, urlProcessor);
#endif
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
            return new InterfaceImplementationLocator(GetClientConfiguration().ExtensionAssembly);
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
