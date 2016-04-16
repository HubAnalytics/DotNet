using System.Collections.Generic;
using HubAnalytics.Core.Model;

namespace HubAnalytics.Core
{
    /// <summary>
    /// Interface that allows buffering telemetry providers to provide events to the dispatcher
    /// </summary>
    public interface ITelemetryEventProvider
    {
        void Cancel();
        IReadOnlyCollection<Event> GetEvents(int batchSize);
    }
}
