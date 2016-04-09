using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HubAnalytics.Core;

namespace HubAnalytics.Ado.Proxies
{
    public class ProxyDbCommand : DbCommand
    {
        private DbCommand _proxiedCommand;
        private readonly IHubAnalyticsClient _hubAnalyticsClient;
        private ProxyDbConnection _dbConnection;

        public ProxyDbCommand(DbCommand command, IHubAnalyticsClient hubAnalyticsClient)
        {
            _proxiedCommand = command;
            _hubAnalyticsClient = hubAnalyticsClient;
        }

        public ProxyDbCommand(DbCommand command, ProxyDbConnection dbConnection, IHubAnalyticsClient hubAnalyticsClient)
        {
            _proxiedCommand = command;
            _dbConnection = dbConnection;
            _hubAnalyticsClient = hubAnalyticsClient;
        }

        public override void Prepare()
        {
            _proxiedCommand.Prepare();
        }

        public override string CommandText {
            get { return _proxiedCommand.CommandText; }
            set { _proxiedCommand.CommandText = value; }
        }

        public override int CommandTimeout {
            get { return _proxiedCommand.CommandTimeout; }
            set { _proxiedCommand.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return _proxiedCommand.CommandType; }
            set { _proxiedCommand.CommandType = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _proxiedCommand.UpdatedRowSource; }
            set { _proxiedCommand.UpdatedRowSource = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return _dbConnection; }
            set
            {
                _dbConnection = value as ProxyDbConnection;
                if (_dbConnection != null)
                {
                    _proxiedCommand.Connection = _dbConnection.ProxiedConnection;
                }
                else
                {
                    _dbConnection = new ProxyDbConnection(value, _hubAnalyticsClient);
                    _proxiedCommand.Connection = _dbConnection.ProxiedConnection;
                }
            }
        }

        protected override DbParameterCollection DbParameterCollection => _proxiedCommand.Parameters;

        protected override DbTransaction DbTransaction
        {
            get { return _proxiedCommand.Transaction; }
            set
            {
                ProxyDbTransaction proxyDbTransaction = value as ProxyDbTransaction;
                if (proxyDbTransaction != null)
                {
                    _proxiedCommand.Transaction = proxyDbTransaction.ProxiedTransaction;
                }
                else
                {
                    proxyDbTransaction = new ProxyDbTransaction(value, _hubAnalyticsClient);
                    _proxiedCommand.Transaction = proxyDbTransaction.ProxiedTransaction;
                }
            }
        }

        public override bool DesignTimeVisible
        {
            get { return _proxiedCommand.DesignTimeVisible; }
            set { _proxiedCommand.DesignTimeVisible = value; }
        }

        public DbCommand ProxiedCommand => _proxiedCommand;

        public override void Cancel()
        {
            _proxiedCommand.Cancel();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _proxiedCommand.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            DateTimeOffset executedAt = DateTimeOffset.UtcNow;
            DbDataReader result;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                result = _proxiedCommand.ExecuteReader(behavior);
            }
            catch (SqlException ex)
            {
                Log(executedAt, sw, false, ex);
                throw;
            }
            catch (Exception)
            {
                Log(executedAt, sw, false);
                throw;
            }

            Log(executedAt, sw, true);

            return new ProxyDbDataReader(result, this, _hubAnalyticsClient);
        }

        public override int ExecuteNonQuery()
        {
            DateTimeOffset executedAt = DateTimeOffset.UtcNow;
           
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int result;

            try
            {
                result = _proxiedCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Log(executedAt, sw, false, ex);
                throw;
            }
            catch (Exception)
            {
                Log(executedAt, sw,false);
                throw;
            }

            Log(executedAt, sw, true);

            return result;
        }

        public override object ExecuteScalar()
        {
            DateTimeOffset executedAt = DateTimeOffset.UtcNow;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            object result;

            try
            {
                result = _proxiedCommand.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                Log(executedAt, sw, false, ex);
                throw;
            }
            catch (Exception)
            {
                Log(executedAt, sw, false);
                throw;
            }

            Log(executedAt, sw, true);

            return result;
        }

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            DateTimeOffset executedAt = DateTimeOffset.UtcNow;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            DbDataReader reader;

            try
            {
                reader = await _proxiedCommand.ExecuteReaderAsync(behavior, cancellationToken);
            }
            catch (SqlException ex)
            {
                Log(executedAt, sw, false, ex);
                throw;
            }
            catch (Exception)
            {
                Log(executedAt, sw, false);
                throw;
            }
            Log(executedAt, sw, true);

            return new ProxyDbDataReader(reader, this, _hubAnalyticsClient);
        }

        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            DateTimeOffset executedAt = DateTimeOffset.UtcNow;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int result;

            try
            {
                result = await _proxiedCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (SqlException ex)
            {
                Log(executedAt, sw, false, ex);
                throw;
            }
            catch (Exception)
            {
                Log(executedAt, sw, false);
                throw;
            }
            
            Log(executedAt, sw,true);
            return result;
        }

        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            DateTimeOffset executedAt = DateTimeOffset.UtcNow;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            object result;

            try
            {
                result = await _proxiedCommand.ExecuteScalarAsync(cancellationToken);
            }
            catch (SqlException ex)
            {
                Log(executedAt, sw, false, ex);
                throw;
            }
            catch (Exception)
            {
                Log(executedAt, sw, false);
                throw;
            }
            
            Log(executedAt, sw, true);
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _proxiedCommand?.Dispose();
            }

            _proxiedCommand = null;
            _dbConnection = null;
            base.Dispose(disposing);
        }

        private void Log(DateTimeOffset executedAt, Stopwatch sw, bool success, SqlException ex = null)
        {
            sw.Stop();
            _hubAnalyticsClient.SqlCommand(executedAt, _proxiedCommand.Connection?.ConnectionString, _proxiedCommand.CommandText, (int)sw.ElapsedMilliseconds, success, ex?.Number);
        }
    }
}
