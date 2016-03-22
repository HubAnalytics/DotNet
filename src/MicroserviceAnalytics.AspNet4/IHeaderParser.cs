using System.Collections.Generic;
using System.Collections.Specialized;

namespace MicroserviceAnalytics.AspNet4
{
    public interface IHeaderParser
    {
        Dictionary<string, string[]> CaptureHeaders(IReadOnlyCollection<string> captureHeaders, NameValueCollection headers);
    }
}
