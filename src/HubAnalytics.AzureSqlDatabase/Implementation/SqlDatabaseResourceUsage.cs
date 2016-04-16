using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubAnalytics.Core.Model;

namespace HubAnalytics.AzureSqlDatabase.Implementation
{
    internal class SqlDatabaseResourceUsage : ISqlDatabaseResourceUsage
    {
        private readonly ConcurrentDictionary<DateTimeOffset, TelemetryItem> _usage = new ConcurrentDictionary<DateTimeOffset, TelemetryItem>();
        private readonly AzureSqlDatabase _azureSqlDatabase;
        private readonly IUsageProvider _usageProvider;
        private readonly ITelemetryItemToEventMapper _mapper;
        private int _concurrentFailures;

        public SqlDatabaseResourceUsage(AzureSqlDatabase azureSqlDatabase, IUsageProvider usageProvider, ITelemetryItemToEventMapper mapper)
        {
            _azureSqlDatabase = azureSqlDatabase;
            _usageProvider = usageProvider;
            _mapper = mapper;
        }

        public string Name => _azureSqlDatabase.Name;

        public async Task<bool> Update()
        {
            try
            {
                IReadOnlyCollection<TelemetryItem> updates = await _usageProvider.Get(_azureSqlDatabase);
                Merge(updates);
                _concurrentFailures = 0;
                return true;
            }
            catch (Exception)
            {
                _concurrentFailures++;
                return false;
            }
        }

        public IReadOnlyCollection<Event> GetEvents()
        {
            List<TelemetryItem> items = new List<TelemetryItem>();
            ICollection<DateTimeOffset> keys = _usage.Keys;

            foreach (DateTimeOffset key in keys)
            {
                // doesn't matter if it gets readded by the parallel gathering process or if one just added is removed
                TelemetryItem item;
                if (_usage.TryRemove(key, out item))
                {
                    items.Add(item);
                }
            }

            string connectionString = _azureSqlDatabase.TransportSecureConnectionString;
            return items.Select(x => _mapper.Map(x, _azureSqlDatabase.PropertyId, _usageProvider.Granularity, _azureSqlDatabase.Name, connectionString)).ToList();
        }

        private void Merge(IReadOnlyCollection<TelemetryItem> mergeItems)
        {
            foreach (TelemetryItem mergeItem in mergeItems)
            {
                _usage.AddOrUpdate(mergeItem.PeriodBegin, mergeItem, (existingKey, existingValue) => mergeItem);
            }
        }

        public int ConcurrentFailures => _concurrentFailures;
    }
}
