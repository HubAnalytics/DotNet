using System.Data.Common;
using HubAnalytics.Ado.Proxies;

namespace HubAnalytics.Ado.Extensions
{
    internal static class DbConnectionExtensions
    {
        public static DbProviderFactory TryGetProviderFactory(this DbConnection connection)
        {
            // If we can pull it out quickly and easily
            ProxyDbConnection proxyDbConnection = connection as ProxyDbConnection;
            if (proxyDbConnection != null)
            {
                return proxyDbConnection.ProviderFactory;
            }

//#if (NET45)
            return DbProviderFactories.GetFactory(connection);
//#else
//            return dbConnection.GetType().GetProperty("ProviderFactory", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dbConnection, null) as DbProviderFactory;
//#endif
        }
    }
}
