using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Linq;

namespace HubAnalytics.EF6
{
    public class InvariantNameResolver : IDbDependencyResolver
    {
        private readonly object _registeredFactoriesLock = new object();
        private Dictionary<string, string> _registeredFactories;

        private Dictionary<string, string> RegisteredFactories
        {
            get
            {
                if (_registeredFactories == null)
                {
                    lock (_registeredFactoriesLock)
                    {
                        if (_registeredFactories == null)
                        {
                            _registeredFactories = Ado.HubAnalytics.Factories
                                .ToDictionary(x => x.Value, x => x.Key);
                        }
                    }
                }

                return _registeredFactories;
            }
        }

        public virtual object GetService(Type type, object key)
        {
            if (type == typeof(IProviderInvariantName))
            {
                var factoryType = key.GetType().AssemblyQualifiedName;

                var factoryName = (string)null;
                if (RegisteredFactories.TryGetValue(factoryType, out factoryName))
                {
                    return new ProviderInvariantName(factoryName);
                }
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            var service = GetService(type, key);

            return service == null ? Enumerable.Empty<object>() : new[] { service };
        }

        private class ProviderInvariantName : IProviderInvariantName
        {
            public ProviderInvariantName(string name)
            {
                Name = name;
            }

            public string Name { get; private set; }
        }
    }
}
