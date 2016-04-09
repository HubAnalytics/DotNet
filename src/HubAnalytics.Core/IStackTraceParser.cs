using System;
using System.Collections.Generic;
using HubAnalytics.Core.Model;

namespace HubAnalytics.Core
{
    public interface IStackTraceParser
    {
        List<StackTraceEntry> Get(Exception ex);
    }
}
