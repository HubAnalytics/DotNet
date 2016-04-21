using System.Configuration;

namespace HubAnalytics.AzureSqlDatabase
{
    public class HubAnalyticsAzureSqlDatabaseConfigurationSection : ConfigurationSection
    {
        public static readonly HubAnalyticsAzureSqlDatabaseConfigurationSection Settings = ConfigurationManager.GetSection("hubAnalyticsAzureSqlDatabaseSettings") as HubAnalyticsAzureSqlDatabaseConfigurationSection ?? new HubAnalyticsAzureSqlDatabaseConfigurationSection();

        [ConfigurationProperty("azureSqlDatabases", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(AzureSqlDatabaseCollection), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public AzureSqlDatabaseCollection AzureSqlDatabases => (AzureSqlDatabaseCollection) this["azureSqlDatabases"];

        [ConfigurationProperty("telemetryIntervalMs", IsRequired = false, DefaultValue = 15000)]
        public int TelemetryIntervalMs
        {
            get { return (int)this["telemetryIntervalMs"]; }
            set { this["telemetryIntervalMs"] = value; }
        }
    }
}
