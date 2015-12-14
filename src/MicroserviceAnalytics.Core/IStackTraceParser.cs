using System;
using System.Collections.Generic;
using MicroserviceAnalytics.Core.Model;

namespace MicroserviceAnalytics.Core
{
    public interface IStackTraceParser
    {
        List<StackTraceEntry> Get(Exception ex);
    }
}
