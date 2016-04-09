using System.Data;
using System.Data.Common;
using System.Diagnostics;
using HubAnalytics.Core;

namespace HubAnalytics.Ado.Proxies
{
    public class ProxyDbDataAdapter : DbDataAdapter
    {
        private readonly DbDataAdapter _proxiedAdapter;
        private readonly IHubAnalyticsClient _hubAnalyticsClient;

        public ProxyDbDataAdapter(DbDataAdapter adapter, IHubAnalyticsClient hubAnalyticsClient)
        {
            _proxiedAdapter = adapter;
            _hubAnalyticsClient = hubAnalyticsClient;
        }

        protected override void Dispose(bool disposing)
        {
            _proxiedAdapter.Dispose();
        }

        public override int Fill(DataSet dataSet)
        {
            if (SelectCommand != null)
            {
                ProxyDbCommand proxyDbCommand = SelectCommand as ProxyDbCommand;
                if (proxyDbCommand != null)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    int result = _proxiedAdapter.Fill(dataSet);
                    sw.Stop();
                    // TODO: log
                    return result;
                }

                _proxiedAdapter.SelectCommand = SelectCommand;
            }

            return _proxiedAdapter.Fill(dataSet);
        }

        public override DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType)
        {
            if (SelectCommand != null)
            {
                _proxiedAdapter.SelectCommand = RetrieveBaseCommand(SelectCommand);
            }
            return _proxiedAdapter.FillSchema(dataSet, schemaType);
        }

        public override IDataParameter[] GetFillParameters()
        {
            return _proxiedAdapter.GetFillParameters();
        }

        public override bool ReturnProviderSpecificTypes
        {
            get { return _proxiedAdapter.ReturnProviderSpecificTypes; }
            set { _proxiedAdapter.ReturnProviderSpecificTypes = value; }
        }

        public override bool ShouldSerializeAcceptChangesDuringFill()
        {
            return _proxiedAdapter.ShouldSerializeAcceptChangesDuringFill();
        }

        public override bool ShouldSerializeFillLoadOption()
        {
            return _proxiedAdapter.ShouldSerializeFillLoadOption();
        }

        public override int Update(DataSet dataSet)
        {
            if (UpdateCommand != null)
            {
                _proxiedAdapter.UpdateCommand = RetrieveBaseCommand(UpdateCommand);
            }

            if (InsertCommand != null)
            {
                _proxiedAdapter.InsertCommand = RetrieveBaseCommand(InsertCommand);
            }

            if (DeleteCommand != null)
            {
                _proxiedAdapter.DeleteCommand = RetrieveBaseCommand(DeleteCommand);
            }

            return _proxiedAdapter.Update(dataSet);
        }

        public override int UpdateBatchSize
        {
            get { return _proxiedAdapter.UpdateBatchSize; }
            set { _proxiedAdapter.UpdateBatchSize = value; }
        }

        private DbCommand RetrieveBaseCommand(DbCommand command)
        {
            ProxyDbCommand typedDbCommand = command as ProxyDbCommand;
            return typedDbCommand?.ProxiedCommand ?? command;
        }
    }
}
