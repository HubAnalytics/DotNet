using System.Data.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Spatial;
using System.Reflection;
using MicroserviceAnalytics.Ado.Proxies;
using MicroserviceAnalytics.Core;
using DbCommandDefinition = System.Data.Entity.Core.Common.DbCommandDefinition;
using DbProviderManifest = System.Data.Entity.Core.Common.DbProviderManifest;
using DbProviderServices = System.Data.Entity.Core.Common.DbProviderServices;

namespace MicroserviceAnalytics.EF6.Proxies
{
    public class ProxyDbProviderServices : DbProviderServices
    {
        private readonly DbProviderServices _proxiedProviderServices;
        private readonly IMicroserviceAnalyticClient _recorder;

//#if (EF5 && NET45) || EF6
        private readonly MethodInfo _setParameterValueMethod;
//#endif

        public ProxyDbProviderServices(DbProviderServices proxiedProviderServices, IMicroserviceAnalyticClient recorder)
        {
            _proxiedProviderServices = proxiedProviderServices;
            _recorder = recorder;

//#if (EF5 && NET45) || EF6
            _setParameterValueMethod = _proxiedProviderServices.GetType().GetMethod("SetParameterValue", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
//#endif
        }

        public override DbCommandDefinition CreateCommandDefinition(DbCommand prototype)
        {
            return new ProxyDbCommandDefinition(_proxiedProviderServices.CreateCommandDefinition(prototype), _recorder);
        }

        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            return new ProxyDbCommandDefinition(_proxiedProviderServices.CreateCommandDefinition(providerManifest, commandTree), _recorder);
        }

        protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            ProxyDbConnection proxyConnection = connection as ProxyDbConnection;
            if (proxyConnection != null)
            {
                connection = proxyConnection.ProxiedConnection;
            }

            _proxiedProviderServices.CreateDatabase(connection, commandTimeout, storeItemCollection);
        }

        protected override string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            return _proxiedProviderServices.CreateDatabaseScript(providerManifestToken, storeItemCollection);
        }

        protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            ProxyDbConnection proxyConnection = connection as ProxyDbConnection;
            if (proxyConnection != null)
            {
                connection = proxyConnection.ProxiedConnection;
            }

            return _proxiedProviderServices.DatabaseExists(connection, commandTimeout, storeItemCollection);
        }

        protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            ProxyDbConnection proxyConnection = connection as ProxyDbConnection;
            if (proxyConnection != null)
            {
                connection = proxyConnection.ProxiedConnection;
            }
            _proxiedProviderServices.DeleteDatabase(connection, commandTimeout, storeItemCollection);
        }

        protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
        {
            return _proxiedProviderServices.GetProviderManifest(manifestToken);
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            var proxyConnection = connection as ProxyDbConnection;
            DbConnection rawConnection = proxyConnection == null ? connection : proxyConnection.ProxiedConnection;
            return _proxiedProviderServices.GetProviderManifestToken(rawConnection);
        }

//#if (EF5 && NET45) || EF6Plus
        protected override DbSpatialDataReader GetDbSpatialDataReader(System.Data.Common.DbDataReader fromReader, string manifestToken)
        {
            var typedReader = fromReader as ProxyDbDataReader;
            if (typedReader != null)
            {
                fromReader = typedReader.ProxiedDataReader;
            }

            return _proxiedProviderServices.GetSpatialDataReader(fromReader, manifestToken);
        }
//#endif

//#if (EF5 && NET45) || EF6
        // SetParameterValue is internal and am unable to call it on the InnerProviderServices from here. 
        // This breaks the provider wrapper when making spatial queries in EF 6.0.1
        // http://stackoverflow.com/questions/19966106/spatial-datareader-and-wrapping-providers-in-ef6  
        protected override void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        { 
            _setParameterValueMethod.Invoke(_proxiedProviderServices, new[] { parameter, parameterType, value });
        }
//#endif

#if EF7Plus
        protected override void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        {
            InnerProviderServices.SetParameterValue(parameter, parameterType, value);
        } 
#endif
    }
}
