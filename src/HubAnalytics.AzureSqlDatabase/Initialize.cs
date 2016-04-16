using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using HubAnalytics.AzureSqlDatabase.Implementation;
using HubAnalytics.Core;

namespace HubAnalytics.AzureSqlDatabase
{
    public static class Initialize
    {
        public static void Attach()
        {
            Attach(new HubAnalyticsClientFactory());
        }

        public static void Attach(IHubAnalyticsClientFactory hubAnalyticsClientFactory)
        {
            List<AzureSqlDatabase> databases = DatabasesFromConfigurationSection();
            if (databases.Count == 0)
            {
                databases.AddRange(from ConnectionStringSettings settings in ConfigurationManager.ConnectionStrings
                    select new AzureSqlDatabase
                    {
                        ConnectionString = settings.ConnectionString, Name = settings.Name
                    });
            }
            TimeSpan interval = TimeSpan.FromMilliseconds(HubAnalyticsAzureSqlDatabaseConfigurationSection.Settings.TelemetryIntervalMs);
            IHubAnalyticsClient hubAnalyticsClient = hubAnalyticsClientFactory.GetClient();
            hubAnalyticsClient.RegisterTelemetryProvider(
                new TelemetryProvider(databases,
                    new SqlByHourProvider(new DataReaderToTelemetryItemMapper()),
                    new SqlByMinuteProvider(new DataReaderToTelemetryItemMapper()),
                    new TelemetryItemToEventMapper(),
                    interval));
        }

        private static List<AzureSqlDatabase> DatabasesFromConfigurationSection()
        {
            List<AzureSqlDatabase> result = new List<AzureSqlDatabase>();
            if (HubAnalyticsAzureSqlDatabaseConfigurationSection.Settings.AzureSqlDatabases.Count > 0)
            {
                result.AddRange(from AzureSqlDatabaseConfigurationElement element in HubAnalyticsAzureSqlDatabaseConfigurationSection.Settings.AzureSqlDatabases
                    select new AzureSqlDatabase
                    {
                        ConnectionString = element.ConnectionString, Name = element.Name
                    });
            }
            return result;
        }
    }
}
