using System;
using System.Data.Common;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using MicroserviceAnalytics.Core;

namespace MicroserviceAnalytics.Ado.Proxies
{
    // base class allows for easy existing proxy check
    public abstract class ProxyDbProviderFactory : DbProviderFactory
    {
        public static IMicroserviceAnalyticClient MicroserviceAnalyticClient { get; set; }
    }

    public class ProxyDbProviderFactory<T> : ProxyDbProviderFactory where T : DbProviderFactory
    {
        private readonly T _proxiedFactory;

        // Required by ADO
        public static readonly ProxyDbProviderFactory<T> Instance = new ProxyDbProviderFactory<T>();

        public ProxyDbProviderFactory()
        {
            var field = typeof(T).GetField("Instance", BindingFlags.Public | BindingFlags.Static);
            if (field == null)
            {
                throw new NotSupportedException("Provider doesn't have Instance property.");
            }

            _proxiedFactory = (T)field.GetValue(null);
        }

        public override bool CanCreateDataSourceEnumerator => _proxiedFactory.CanCreateDataSourceEnumerator;

        public override DbCommand CreateCommand()
        {
            return new ProxyDbCommand(_proxiedFactory.CreateCommand(), MicroserviceAnalyticClient);
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return _proxiedFactory.CreateCommandBuilder();
        }

        public override DbConnection CreateConnection()
        {
            DbConnection connection = _proxiedFactory.CreateConnection();
            return new ProxyDbConnection(connection, MicroserviceAnalyticClient);
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return _proxiedFactory.CreateConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new ProxyDbDataAdapter(_proxiedFactory.CreateDataAdapter(), MicroserviceAnalyticClient);
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return _proxiedFactory.CreateDataSourceEnumerator();
        }

        public override DbParameter CreateParameter()
        {
            return _proxiedFactory.CreateParameter();
        }

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            return _proxiedFactory.CreatePermission(state);
        }
    } 
}
