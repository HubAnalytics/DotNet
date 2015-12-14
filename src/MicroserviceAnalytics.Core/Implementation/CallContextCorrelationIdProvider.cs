#if DNXCORE50
using System.Threading;
#else
using System.Runtime.Remoting.Messaging;
#endif

namespace MicroserviceAnalytics.Core.Implementation
{
    internal class CallContextCorrelationIdProvider : ICorrelationIdProvider
    {
        
#if DNXCORE50
        private static readonly AsyncLocal<string> CallData = new AsyncLocal<string>();

        public string CorrelationId
        {
            get
            {
                string correlationId = CallData.Value;
                return correlationId;
            }
            set
            {
                CallData.Value = value;
            }
        }
#else
        private readonly string _callContextKey;

        public CallContextCorrelationIdProvider(string callContextKey = Constants.CorrelationIdKey)
        {
            _callContextKey = callContextKey;
        }

        public string CorrelationId
        {
            get
            {
                object correlationId = CallContext.LogicalGetData(_callContextKey);

                return (string) correlationId;
            }
            set
            {
                CallContext.LogicalSetData(_callContextKey, value);
            }
        }
#endif
    }
}
