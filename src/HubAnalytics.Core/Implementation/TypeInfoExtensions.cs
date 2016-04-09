using System.Linq;
using System.Reflection;

namespace HubAnalytics.Core.Implementation
{
    public static class TypeInfoExtensions
    {
#if DNXCORE50
        public static TypeInfo[] GetGenericArguments(this TypeInfo typeInfo)
        {
            return typeInfo.GenericTypeArguments.Select(x => x.GetTypeInfo()).ToArray();
        }
#endif
    }
}
