using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HubAnalytics.Core;
using HubAnalytics.Core.Model;

namespace HubAnalytics.AzureSqlDatabase.Implementation
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    internal class TelemetryProvider : ITelemetryEventProvider
    {
        private readonly TimeSpan _interval;
        private const int MaxConcurrentRetries = 10;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<SqlDatabaseResourceUsage> _usageSets = new List<SqlDatabaseResourceUsage>();

        public TelemetryProvider(IReadOnlyCollection<AzureSqlDatabase> databases,
            IUsageProvider sqlByHourProvider,
            IUsageProvider sqlByMinuteProvider,
            ITelemetryItemToEventMapper mapper,
            TimeSpan interval)
        {
            _interval = interval;
            _usageSets.AddRange(databases.Select(x => new SqlDatabaseResourceUsage(x, sqlByMinuteProvider, mapper)).ToList());
            _usageSets.AddRange(databases.Select(x => new SqlDatabaseResourceUsage(x, sqlByHourProvider, mapper)).ToList());

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                await BackgroundPush();
            });
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// This method is the reason the telemetry provider needs to be thread safe. It can be called at any time including while the resource
        /// usage collections are being updated
        /// </summary>
        public IReadOnlyCollection<Event> GetEvents(int batchSize)
        {
            List<Event> events = new List<Event>();
            foreach (SqlDatabaseResourceUsage usageSet in _usageSets)
            {
                events.AddRange(usageSet.GetEvents());
            }

            return events;
        }

        private async Task BackgroundPush()
        {
            bool shouldContinue = !_cancellationTokenSource.IsCancellationRequested;
            while (shouldContinue)
            {
                foreach (SqlDatabaseResourceUsage usageSet in _usageSets.ToArray())
                {
                    if (!await usageSet.Update())
                    {
                        if (usageSet.ConcurrentFailures == MaxConcurrentRetries)
                        {
                            System.Diagnostics.Trace.WriteLine($"{MaxConcurrentRetries} of {usageSet.AzureSqlDatabase.Name}. Cancelling telemetry logging.", "Error");
                            _usageSets.Remove(usageSet);
                        }
                    }
                }

                if (_usageSets.Count == 0)
                {
                    shouldContinue = false;
                }
                else 
                {
                    await Task.Delay(_interval, _cancellationTokenSource.Token);
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        shouldContinue = false;
                    }
                }
            }
        }
    }
}
