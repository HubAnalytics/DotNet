using System.Configuration;

namespace HubAnalytics.AzureSqlDatabase
{
    public class AzureSqlDatabaseCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AzureSqlDatabaseConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AzureSqlDatabaseConfigurationElement) element).Name;
        }

        public void Add(AzureSqlDatabaseConfigurationElement element)
        {
            BaseAdd(element);
        }

        public void Remove(AzureSqlDatabaseCollection element)
        {
            BaseRemove(element);
        }

        public void Clear()
        {
            BaseClear();
        }
    }
}
