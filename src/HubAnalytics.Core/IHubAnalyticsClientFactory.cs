using System.Collections.Generic;
using HubAnalytics.Core.Helpers;

namespace HubAnalytics.Core
{
    public interface IHubAnalyticsClientFactory
    {
        IHubAnalyticsClient GetClient();
        IClientConfiguration GetClientConfiguration();
        IContextualIdProvider GetCorrelationIdProvider();
        IEnvironmentCapture GetEnvironmentCapture();
        IStackTraceParser GetStackTraceParser();
        IRuntimeProviderDiscoveryService GetRuntimeProviderDiscoveryService();
        IReadOnlyCollection<IDataCapturePlugin> DataCapturePlugins { get; }
    }
}