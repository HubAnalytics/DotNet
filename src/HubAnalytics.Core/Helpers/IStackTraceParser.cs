using System;
using System.Collections.Generic;
using HubAnalytics.Core.Model;

namespace HubAnalytics.Core.Helpers
{
    public interface IStackTraceParser
    {
        List<StackTraceEntry> Get(Exception ex);
    }
}
