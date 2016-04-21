#if DNXCORE50
using System.Reflection;
using System.Linq;
#endif

namespace HubAnalytics.Core.Helpers
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
