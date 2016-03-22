using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.PlatformAbstractions;

namespace DiscoverySample
{
    public interface IDoSomething
    {
        void It();
    }

    public class Class1
    {
        public IEnumerable<Type> Discover()
        {
            List<Type> candidates = new List<Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                candidates.AddRange(assembly.GetTypes().Where(t => typeof (IDoSomething).IsAssignableFrom(t) && t.GetTypeInfo().IsClass));
            }

            var libs = PlatformServices.Default.LibraryManager.GetReferencingLibraries("DiscoverySample")
                .SelectMany(lib => lib.Assemblies)
                .Select(info => Assembly.Load(new AssemblyName(info.Name)));

            
            foreach (Assembly a in libs)
            {
                candidates.AddRange(a.GetTypes().Where(t => typeof(IDoSomething).IsAssignableFrom(t) && t.GetTypeInfo().IsClass));
            }


            return candidates;

            
        }
    }
}
