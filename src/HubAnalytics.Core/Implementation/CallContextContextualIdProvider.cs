#if DNXCORE50
using System.Threading;

#else
using System.Runtime.Remoting.Messaging;
#endif

namespace HubAnalytics.Core.Implementation
{
    internal class CallContextContextualIdProvider : IContextualIdProvider
    {
        
#if DNXCORE50
        private static readonly AsyncLocal<string> CorrelationIdData = new AsyncLocal<string>();
        private static readonly AsyncLocal<string> UserIdData = new AsyncLocal<string>();
        private static readonly AsyncLocal<string> SessionIdData = new AsyncLocal<string>();

        public string CorrelationId
        {
            get
            {
                string correlationId = CorrelationIdData.Value;
                return correlationId;
            }
            set
            {
                CorrelationIdData.Value = value;
            }
        }

        public string UserId
        {
            get
            {
                string userId = UserIdData.Value;
                return userId;
            }
            set
            {
                UserIdData.Value = value;
            }
        }

        public string SessionId
        {
            get
            {
                string sessionId = SessionIdData.Value;
                return sessionId;
            }
            set
            {
                SessionIdData.Value = value;
            }
        }
        
#else
        private readonly string _callContextKey;
        private readonly string _userIdKey;
        private readonly string _sessionIdKey;

        public CallContextContextualIdProvider(string callContextKey = Constants.CorrelationIdKey, string userIdKey = Constants.UserIdKey, string sessionIdKey = Constants.SessionIdKey)
        {
            _callContextKey = callContextKey;
            _userIdKey = userIdKey;
            _sessionIdKey = sessionIdKey;
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

        public string UserId
        {
            get
            {
                object userId = CallContext.LogicalGetData(_userIdKey);

                return (string) userId;
            }
            set
            {
                CallContext.LogicalSetData(_userIdKey, value);
            }
        }

        public string SessionId
        {
            get
            {
                object sessionId = CallContext.LogicalGetData(_sessionIdKey);

                return (string) sessionId;
            }
            set
            {
                CallContext.LogicalSetData(_sessionIdKey, value);
            }
        }
#endif
    }
}
