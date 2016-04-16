using System.Configuration;

namespace HubAnalytics.AzureSqlDatabase
{
    public class AzureSqlDatabaseConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("connectionString", IsRequired = true, IsKey = true)]
        public string ConnectionString
        {
            get { return (string)this["connectionString"]; }
            set { this["connectionString"] = value; }
        }

        /// <summary>
        /// This can be used to collect telemetry from multiple SQL databases within a single process using a single gathering client
        /// but allowing each one to be logged against a different application.
        /// </summary>
        [ConfigurationProperty("propertyId", IsRequired = true, DefaultValue = "")]
        [StringValidator]
        public string PropertyId
        {
            get { return (string)this["propertyId"]; }
            set { this["propertyId"] = value; }
        }
    }
}
