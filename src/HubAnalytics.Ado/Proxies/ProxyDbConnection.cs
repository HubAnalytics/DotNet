using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using HubAnalytics.Ado.Extensions;
using HubAnalytics.Core;

namespace HubAnalytics.Ado.Proxies
{
    public class ProxyDbConnection : DbConnection
    {
        private DbConnection _proxiedConnection;
        private DbProviderFactory _factory;
        private readonly IHubAnalyticsClient _hubAnalyticsClient;
        
        public ProxyDbConnection(DbConnection connection, IHubAnalyticsClient hubAnalyticsClient)
        {
            _proxiedConnection = connection;
            _factory = connection.TryGetProviderFactory();
            _hubAnalyticsClient = hubAnalyticsClient;
            _proxiedConnection.StateChange += StateChangeHandler;
        }

        public ProxyDbConnection(DbConnection connection, DbProviderFactory factory)
        {
            _proxiedConnection = connection;
            _factory = factory;
            _proxiedConnection.StateChange += StateChangeHandler;
        }

        internal DbProviderFactory ProviderFactory => _factory;

        public DbConnection ProxiedConnection => _proxiedConnection;

        

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new ProxyDbTransaction(_proxiedConnection.BeginTransaction(isolationLevel), _hubAnalyticsClient);
        }

        public override void Close()
        {
            _proxiedConnection.Close();
        }

        public override void ChangeDatabase(string databaseName)
        {
            _proxiedConnection.ChangeDatabase(databaseName);
        }

        public override void Open()
        {
            _proxiedConnection.Open();
        }

        public override string ConnectionString
        {
            get { return _proxiedConnection.ConnectionString; }
            set { _proxiedConnection.ConnectionString = value; }
        }

        public override string Database => _proxiedConnection.Database;

        public override ConnectionState State => _proxiedConnection.State;
        public override string DataSource => _proxiedConnection.DataSource;
        public override string ServerVersion => _proxiedConnection.ServerVersion;

        protected override DbCommand CreateDbCommand()
        {
            return new ProxyDbCommand(_proxiedConnection.CreateCommand(), this, _hubAnalyticsClient);
        }

        public override event StateChangeEventHandler StateChange
        {
            add
            {
                if (_proxiedConnection != null)
                {
                    _proxiedConnection.StateChange += value;
                }
            }
            remove
            {
                if (_proxiedConnection != null)
                {
                    _proxiedConnection.StateChange -= value;
                }
            }
        }

        public override ISite Site
        {
            get { return _proxiedConnection.Site; }
            set { _proxiedConnection.Site = value; }
        }

        protected override DbProviderFactory DbProviderFactory => _factory;

        public override int ConnectionTimeout => _proxiedConnection.ConnectionTimeout;

        protected override void Dispose(bool disposing)
        {
            if (disposing && _proxiedConnection != null)
            {
                _proxiedConnection.Dispose();
                _proxiedConnection.StateChange -= StateChangeHandler;
            }

            base.Dispose(disposing);
            _proxiedConnection = null;
            _factory = null;
        }

        public override DataTable GetSchema()
        {
            return _proxiedConnection.GetSchema();
        }

        public override DataTable GetSchema(string collectionName)
        {
            return _proxiedConnection.GetSchema(collectionName);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return _proxiedConnection.GetSchema(collectionName, restrictionValues);
        }

        public override void EnlistTransaction(System.Transactions.Transaction transaction)
        {
            _proxiedConnection.EnlistTransaction(transaction);
        }

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            await _proxiedConnection.OpenAsync(cancellationToken);
        }

        private void StateChangeHandler(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Open)
            {
                // TODO: start logging
            }
            else if (e.CurrentState == ConnectionState.Closed)
            {
                // TODO: end logging
            }
        }
    }
}
