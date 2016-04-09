using System.Collections.Generic;
using System.Collections.Specialized;

namespace HubAnalytics.AspNet4
{
    public interface IHeaderParser
    {
        Dictionary<string, string[]> CaptureHeaders(IReadOnlyCollection<string> captureHeaders, NameValueCollection headers);
    }
}
