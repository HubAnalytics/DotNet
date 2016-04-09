using System.Data;
using System.Data.Common;
using HubAnalytics.Core;

namespace HubAnalytics.Ado.Proxies
{
    public class ProxyDbTransaction : DbTransaction
    {
        private readonly DbTransaction _proxiedTransaction;
        private readonly IMicroserviceAnalyticClient _microserviceAnalyticClient;

        public ProxyDbTransaction(DbTransaction transaction, IMicroserviceAnalyticClient microserviceAnalyticClient)
        {
            _proxiedTransaction = transaction;
            _microserviceAnalyticClient = microserviceAnalyticClient;
        }

        public override void Commit()
        {
            _proxiedTransaction.Commit();
        }

        public override void Rollback()
        {
            _proxiedTransaction.Rollback();
        }

        protected override DbConnection DbConnection => _proxiedTransaction.Connection;
        public override IsolationLevel IsolationLevel => _proxiedTransaction.IsolationLevel;
        public DbTransaction ProxiedTransaction => _proxiedTransaction;
    }
}
