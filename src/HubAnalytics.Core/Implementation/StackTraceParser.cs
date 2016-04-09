using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using HubAnalytics.Core.Model;

namespace HubAnalytics.Core.Implementation
{
    internal class StackTraceParser : IStackTraceParser
    {
        public List<StackTraceEntry> Get(Exception ex)
        {
            StackTrace stackTrace = new StackTrace(ex, true);
            StackFrame[] frames = stackTrace.GetFrames();
            return frames?.Select(x =>
            {
                MethodBase methodBase = x.GetMethod();
                string className = GetClassName(methodBase);
                int lineNumber = x.GetFileLineNumber();
                if (lineNumber == 0)
                {
                    lineNumber = x.GetILOffset();
                }

                return new StackTraceEntry
                {
                    Assembly = "",
                    Class = className,
                    Line = lineNumber,
                    Method = GetMethodName(methodBase)
                };
            }).ToList();
        }

        private static string GetClassName(MethodBase methodBase)
        {
            if (methodBase.DeclaringType == null)
            {
                return "(unknown)";
            }

            return GetClassName(methodBase.DeclaringType);
        }

        private static string GetClassName(Type type)
        {
            StringBuilder sb = new StringBuilder(type.FullName);
            if (type.GetTypeInfo().IsGenericType)
            {
                sb.Append("<");
                bool first = true;
                foreach (TypeInfo typeParameter in type.GetTypeInfo().GetGenericArguments())
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                    sb.Append(GetClassName(typeParameter.AsType()));
                }
                sb.Append(">");
            }
            return sb.ToString();
        }

        private static string GetMethodName(MethodBase methodBase)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(methodBase.Name);

            MethodInfo methodInfo = methodBase as MethodInfo;
            if (methodInfo != null && methodInfo.IsGenericMethod)
            {
                
            }

            return sb.ToString();
        }
    }
}
