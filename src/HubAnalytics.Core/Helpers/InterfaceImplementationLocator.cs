using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

#if DNXCORE50
using Microsoft.Extensions.PlatformAbstractions;
#endif

namespace HubAnalytics.Core.Helpers
{
    public class InterfaceImplementationLocator : IInterfaceImplementationLocator
    {
        private static readonly object AssembliesLock = new object();
        private static IReadOnlyCollection<Assembly> _assemblies = null;

        public IReadOnlyCollection<Type> Implements<T>()
        {
            return Implements(typeof (T));
        }

        public IReadOnlyCollection<Type> Implements(Type type)
        {
            IReadOnlyCollection<Assembly> assemblies = GetAssemblies();
            List<Type> candidateTypes = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                candidateTypes.AddRange(
                    assembly.GetTypes()
                        .Where(t => type.IsAssignableFrom(t) && t.GetTypeInfo().IsClass));
            }
            return candidateTypes;
        }

        private IReadOnlyCollection<Assembly> GetAssemblies()
        {
            if (_assemblies == null)
            {
                lock (AssembliesLock)
                {
                    if (_assemblies == null)
                    {
                        _assemblies = LoadAndGetAssemblies();
                    }
                }
            }
            return _assemblies;
        }

        private static IReadOnlyCollection<Assembly> LoadAndGetAssemblies()
        {
#if DNXCORE50
            IReadOnlyCollection<Assembly> assemblies = PlatformServices.Default.LibraryManager.GetReferencingLibraries(
                "HubAnalytics.Core")
                .SelectMany(lib => lib.Assemblies)
                .Select(info => Assembly.Load(new AssemblyName(info.Name))).ToList();
            return assemblies;
#else
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();
            var baseDirectory = Path.GetDirectoryName(typeof (InterfaceImplementationLocator).Assembly.Location) ?? AppDomain.CurrentDomain.BaseDirectory;
            var referencedPaths = Directory.GetFiles(baseDirectory, "HubAnalytics.*.dll");
            var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
            toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));

            return AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("HubAnalytics.")).ToList();
#endif

        }
    }
}
