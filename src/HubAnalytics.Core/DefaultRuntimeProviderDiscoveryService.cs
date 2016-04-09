using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HubAnalytics.Core.Implementation;
using Microsoft.Extensions.PlatformAbstractions;

namespace HubAnalytics.Core
{
    public class DefaultRuntimeProviderDiscoveryService :IRuntimeProviderDiscoveryService
    {
        private static readonly object UserIdDiscoveryLockObject = new object();
        private static IUserIdProvider _discoveredUserIdProvider;
        private static readonly object SessionIdDiscoveryLockObject = new object();
        private static ISessionIdProvider _discoveredSessionIdProvider;

        public IUserIdProvider UserIdProvider
        {
            get
            {
                if (_discoveredUserIdProvider == null)
                {
                    lock (UserIdDiscoveryLockObject)
                    {
                        if (_discoveredUserIdProvider != null)
                        {
                            return _discoveredUserIdProvider;
                        }
                        IReadOnlyCollection<Assembly> assemblies = GetLoadedAssemblies();
                        List<Type> candidateTypes = new List<Type>();
                        foreach (Assembly assembly in assemblies)
                        {
                            candidateTypes.AddRange(
                                assembly.GetTypes()
                                    .Where(t => typeof (IUserIdProvider).IsAssignableFrom(t) && t.GetTypeInfo().IsClass));
                        }

                        // Look for providers in this order:
                        //  1. a candidate type implemented by an external party
                        //  2. a candidate type not in HubAnalytics.Core
                        //  3. fallback to guid generation
                        var discoveredType = FindExternalToLibraryType(candidateTypes) ??
                                             (FindExternalToCoreType(candidateTypes) ?? typeof (NewGuidUserIdProvider));

                        _discoveredUserIdProvider = (IUserIdProvider)Activator.CreateInstance(discoveredType);
                    }
                }
                return _discoveredUserIdProvider;
            }
        }

        public ISessionIdProvider SessionIdProvider
        {
            get
            {
                if (_discoveredSessionIdProvider == null)
                {
                    lock (SessionIdDiscoveryLockObject)
                    {
                        if (_discoveredSessionIdProvider != null)
                        {
                            return _discoveredSessionIdProvider;
                        }
                        IReadOnlyCollection<Assembly> assemblies = GetLoadedAssemblies();
                        List<Type> candidateTypes = new List<Type>();
                        foreach (Assembly assembly in assemblies)
                        {
                            candidateTypes.AddRange(
                                assembly.GetTypes()
                                    .Where(t => typeof (ISessionIdProvider).IsAssignableFrom(t) && t.GetTypeInfo().IsClass));
                        }

                        // Look for providers in this order:
                        //  1. a candidate type implemented by an external party
                        //  2. a candidate type not in HubAnalytics.Core
                        //  3. fallback to guid generation
                        var discoveredType = FindExternalToLibraryType(candidateTypes) ??
                                             (FindExternalToCoreType(candidateTypes) ?? typeof (NewGuidSessionIdProvider));

                        _discoveredSessionIdProvider = (ISessionIdProvider)Activator.CreateInstance(discoveredType);
                    }
                }
                return _discoveredSessionIdProvider;
            }
        }

        private static Type FindExternalToLibraryType(IReadOnlyCollection<Type> candidateTypes)
        {
            return candidateTypes.FirstOrDefault(x => !x.AssemblyQualifiedName.StartsWith("HubAnalytics."));
        }

        private static Type FindExternalToCoreType(IReadOnlyCollection<Type> candidateTypes)
        {
            return candidateTypes.FirstOrDefault(x => !x.AssemblyQualifiedName.StartsWith("HubAnalytics.Core"));
        }

        private static IReadOnlyCollection<Assembly> GetLoadedAssemblies()
        {
            IReadOnlyCollection<Assembly> assemblies;
#if DNXCORE50
            assemblies = PlatformServices.Default.LibraryManager.GetReferencingLibraries(
                "HubAnalytics.Core")
                .SelectMany(lib => lib.Assemblies)
                .Select(info => Assembly.Load(new AssemblyName(info.Name))).ToList();
#else
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
#endif
            return assemblies;
        }

               
    }
}
