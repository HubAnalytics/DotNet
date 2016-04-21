using System;
using System.Collections.Generic;

namespace HubAnalytics.Core.Helpers
{
    public interface IInterfaceImplementationLocator
    {
        IReadOnlyCollection<Type> Implements<T>();
        IReadOnlyCollection<Type> Implements(Type type);
    }
}