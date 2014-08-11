using System;
using System.Collections.Generic;
using DiceIoC.Lifetimes;

namespace DiceIoC
{
    public interface IContainer
    {
        T Resolve<T>(string name);
        object Resolve(Type serviceType, string name);

        T Resolve<T>();
        object Resolve(Type serviceType);

        bool TryResolve<T>(string name, out T resolved);
        bool TryResolve(Type serviceType, string name, out object resolved);

        bool TryResolve<T>(out T resolved);
        bool TryResolve(Type serviceType, out object resolved);

        IEnumerable<T> ResolveAll<T>();
        IEnumerable<object> ResolveAll(Type serviceType);

        IContainer InScope(IScopedLifetime scope);

        IDictionary<int, object> PerResolveObjects { get; }
        IScopedLifetime CurrentScope { get; }
    }
}
