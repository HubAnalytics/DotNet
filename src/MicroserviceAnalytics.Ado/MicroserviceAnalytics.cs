using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using MicroserviceAnalytics.Ado.Proxies;
using MicroserviceAnalytics.Core;

namespace MicroserviceAnalytics.Ado
{
    public static class MicroserviceAnalytics
    {
        public static readonly Dictionary<string, string> Factories = new Dictionary<string, string>();

        public static void Attach()
        {
            Attach(new MicroserviceAnalyticClientFactory());
        }

        public static void Attach(MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory)
        {
            IMicroserviceAnalyticClient microserviceAnalyticClient = microserviceAnalyticClientFactory.GetClient();
            ProxyDbProviderFactory.MicroserviceAnalyticClient = microserviceAnalyticClient;

            // force initialization
            try
            {
                DbProviderFactories.GetFactory("init");
            }
            catch (ArgumentException)
            {
            }

            DataTable registrations = FindDbProviderFactoryTable();
            List<DataRow> rows = registrations.Rows.Cast<DataRow>().ToList(); // copy to list as we are going to modify
            foreach (DataRow row in rows)
            {
                DbProviderFactory factory;
                try
                {
                    factory = DbProviderFactories.GetFactory(row);
                }
                catch (Exception)
                {
                    // we can't find the factory
                    continue;
                }

                // if we've already proxied it then carry on
                if (factory is ProxyDbProviderFactory)
                {
                    continue;
                }

                var proxyType = typeof(ProxyDbProviderFactory<>).MakeGenericType(factory.GetType());

                Factories.Add(row["InvariantName"].ToString(), row["AssemblyQualifiedName"].ToString());

                var newRow = registrations.NewRow();
                newRow["Name"] = row["Name"];
                newRow["Description"] = row["Description"];
                newRow["InvariantName"] = row["InvariantName"];
                newRow["AssemblyQualifiedName"] = proxyType.AssemblyQualifiedName;

                registrations.Rows.Remove(row);
                registrations.Rows.Add(newRow);
            }
        }

        public static DataTable FindDbProviderFactoryTable()
        {
            var providerFactories = typeof(DbProviderFactories);
            var providerField = providerFactories.GetField("_configTable", BindingFlags.NonPublic | BindingFlags.Static) ?? providerFactories.GetField("_providerTable", BindingFlags.NonPublic | BindingFlags.Static);
            var registrations = providerField.GetValue(null);
            return registrations is DataSet ? ((DataSet)registrations).Tables["DbProviderFactories"] : (DataTable)registrations;
        }
    }
}
