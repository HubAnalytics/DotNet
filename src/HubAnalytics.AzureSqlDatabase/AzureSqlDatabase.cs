using System.Data.Common;

namespace HubAnalytics.AzureSqlDatabase
{
    public class AzureSqlDatabase
    {
        public string Name { get; set; }

        public string ConnectionString { get; set; }

        public string PropertyId { get; set; }

        public string TransportSecureConnectionString
        {
            get
            {
                DbConnectionStringBuilder connectionStringBuilder = new DbConnectionStringBuilder
                {
                    ConnectionString = ConnectionString
                };
                connectionStringBuilder.Remove("Password");
                connectionStringBuilder.Remove("Pwd");
                return connectionStringBuilder.ConnectionString;
            }
        }
    }
}
