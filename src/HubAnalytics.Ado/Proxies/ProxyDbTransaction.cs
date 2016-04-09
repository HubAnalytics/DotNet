using System.Data;
using System.Data.Common;
using HubAnalytics.Core;

namespace HubAnalytics.Ado.Proxies
{
    public class ProxyDbTransaction : DbTransaction
    {
        private readonly DbTransaction _proxiedTransaction;
        private readonly IHubAnalyticsClient _hubAnalyticsClient;

        public ProxyDbTransaction(DbTransaction transaction, IHubAnalyticsClient hubAnalyticsClient)
        {
            _proxiedTransaction = transaction;
            _hubAnalyticsClient = hubAnalyticsClient;
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
