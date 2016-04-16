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
    }
}
