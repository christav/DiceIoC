using System;
using System.Collections.Generic;
using System.Linq;
using DiceIoC.Lifetimes;

namespace DiceIoC
{
    public abstract class Container
    {
        public T Resolve<T>(string name)
        {
            return (T) Resolve(typeof (T), name);
        }

        public abstract object Resolve(Type serviceType, string name);

        public T Resolve<T>()
        {
            return (T) Resolve(typeof (T), null);
        }

        public object Resolve(Type serviceType)
        {
            return Resolve(serviceType, null);
        }

        public bool TryResolve<T>(string name, out T resolved)
        {
            object result;
            bool succeeded = TryResolve(typeof (T), name, out result);
            if (succeeded)
            {
                resolved = (T) result;
            }
            else
            {
                resolved = default(T);
            }
            return succeeded;
        }

        public abstract bool TryResolve(Type serviceType, string name, out object resolved);

        public bool TryResolve<T>(out T resolved)
        {
            return TryResolve(null, out resolved);
        }

        public bool TryResolve(Type serviceType, out object resolved)
        {
            return TryResolve(serviceType, null, out resolved);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return ResolveAll(typeof(T)).Cast<T>();
        }

        public abstract IEnumerable<object> ResolveAll(Type serviceType);

        public abstract Container InScope(IScopedLifetime scope);

        public abstract IScopedLifetime CurrentScope { get; }
        public abstract IDictionary<int, object> PerResolveObjects { get; }
    }
}
