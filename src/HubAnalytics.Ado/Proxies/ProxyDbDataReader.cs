using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using HubAnalytics.Core;

namespace HubAnalytics.Ado.Proxies
{
    public class ProxyDbDataReader : DbDataReader
    {
        private DbDataReader _proxiedDataReader;
        private readonly IHubAnalyticsClient _hubAnalyticsClient;

        public ProxyDbDataReader(DbDataReader dataReader, DbCommand command, IHubAnalyticsClient hubAnalyticsClient)
        {
            _proxiedDataReader = dataReader;
            _hubAnalyticsClient = hubAnalyticsClient;
        }

        public override void Close()
        {
            _proxiedDataReader.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _proxiedDataReader.Dispose();
            }
            // Order of the below is important
            base.Dispose(disposing);
            _proxiedDataReader = null;
        }

        public override string GetName(int ordinal)
        {
            return _proxiedDataReader.GetName(ordinal);
        }

        public override int GetValues(object[] values)
        {
            return _proxiedDataReader.GetValues(values);
        }

        public override bool IsDBNull(int ordinal)
        {
            return _proxiedDataReader.IsDBNull(ordinal);
        }

        public override int FieldCount => _proxiedDataReader.FieldCount;

        public override object this[int ordinal] => _proxiedDataReader[ordinal];

        public override object this[string name] => _proxiedDataReader[name];

        public override bool HasRows => _proxiedDataReader.HasRows;
        public override bool IsClosed => _proxiedDataReader.IsClosed;
        public override int RecordsAffected => _proxiedDataReader.RecordsAffected;

        public override bool NextResult()
        {
            return _proxiedDataReader.NextResult();
        }

        public override bool Read()
        {
            return _proxiedDataReader.Read();
        }

        public override int Depth => _proxiedDataReader.Depth;
        public DbDataReader ProxiedDataReader => _proxiedDataReader;

        public override int GetOrdinal(string name)
        {
            return _proxiedDataReader.GetOrdinal(name);
        }

        public override bool GetBoolean(int ordinal)
        {
            return _proxiedDataReader.GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return _proxiedDataReader.GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return _proxiedDataReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            return _proxiedDataReader.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return _proxiedDataReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override Guid GetGuid(int ordinal)
        {
            return _proxiedDataReader.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return _proxiedDataReader.GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return _proxiedDataReader.GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return _proxiedDataReader.GetInt64(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return _proxiedDataReader.GetDateTime(ordinal);
        }

        public override string GetString(int ordinal)
        {
            return _proxiedDataReader.GetString(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return _proxiedDataReader.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return _proxiedDataReader.GetDouble(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return _proxiedDataReader.GetFloat(ordinal);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return _proxiedDataReader.GetDataTypeName(ordinal);
        }

        public override Type GetFieldType(int ordinal)
        {
            return _proxiedDataReader.GetFieldType(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            return _proxiedDataReader.GetValue(ordinal);
        }

        public override IEnumerator GetEnumerator()
        {
            return _proxiedDataReader.GetEnumerator();
        }

        public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return await _proxiedDataReader.ReadAsync(cancellationToken);
        }

#if NET45 || NET46
        public override DataTable GetSchemaTable()
        {
            return _proxiedDataReader.GetSchemaTable();
        }
#endif
    }
}
