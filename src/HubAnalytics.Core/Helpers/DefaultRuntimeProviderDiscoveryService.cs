using System;
using System.Collections.Generic;
using System.Linq;
using HubAnalytics.Core.Implementation;

#if DNXCORE50
using Microsoft.Extensions.PlatformAbstractions;
#endif

namespace HubAnalytics.Core.Helpers
{
    public class DefaultRuntimeProviderDiscoveryService :IRuntimeProviderDiscoveryService
    {
        private readonly IInterfaceImplementationLocator _interfaceImplementationLocator;

        public DefaultRuntimeProviderDiscoveryService(IInterfaceImplementationLocator interfaceImplementationLocator)
        {
            _interfaceImplementationLocator = interfaceImplementationLocator;
        }

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
                        IReadOnlyCollection<Type> candidateTypes = _interfaceImplementationLocator.Implements<IUserIdProvider>();
                        
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
                        IReadOnlyCollection<Type> candidateTypes = _interfaceImplementationLocator.Implements<ISessionIdProvider>();

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

        

               
    }
}
