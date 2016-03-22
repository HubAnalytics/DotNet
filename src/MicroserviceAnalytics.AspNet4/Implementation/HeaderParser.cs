using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace MicroserviceAnalytics.AspNet4.Implementation
{
    public class HeaderParser : IHeaderParser
    {
        public Dictionary<string, string[]> CaptureHeaders(IReadOnlyCollection<string> captureHeaders, NameValueCollection headers)
        {
            Dictionary<string, string[]> capturedHeaders = null;
            if (captureHeaders != null && captureHeaders.Any())
            {
                capturedHeaders = new Dictionary<string, string[]>();
                if (captureHeaders.First() == "*")
                {
                    foreach (string key in headers.AllKeys)
                    {
                        string[] values = headers.GetValues(key);
                        capturedHeaders[key] = values;
                    }
                }
                else
                {
                    foreach (string headerName in captureHeaders)
                    {
                        string[] values = headers.GetValues(headerName);
                        if (values != null && values.Any())
                        {
                            capturedHeaders.Add(headerName, values);
                        }
                    }
                }
            }
            return capturedHeaders;
        }
    }
}
