using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceIoC
{
    public class Container
    {
        private readonly Dictionary<RegistrationKey, Func<Container, string, Type, object>> factories;
        private readonly Catalog catalog;
        private readonly object factoriesLock = new object();

        internal Container(Catalog catalog, Dictionary<RegistrationKey, Func<Container, string, Type, object>> factories)
        {
            this.catalog = catalog;
            this.factories = factories;
        }

        public Catalog Catalog { get { return catalog; } }

        public T Resolve<T>(string name)
        {
            T resolved;
            if (!TryResolve(name, out resolved))
            {
                throw new ArgumentException(string.Format("The Name/Type {0}/{1} is not registered", name,
                                                          typeof (T).Name));
            }
            return resolved;
        }

        public T Resolve<T>()
        {
            return Resolve<T>(null);
        }

        public bool TryResolve<T>(string name, out T resolved)
        {
            resolved = default(T);
            object result;
            bool succeeded = TryResolve(MakeKey<T>(name), this, out result);
            if (succeeded)
            {
                resolved = (T) result;
            }
            return succeeded;
        }

        public bool TryResolve<T>(out T resolved)
        {
            return TryResolve(null, out resolved);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            lock (factoriesLock)
            {
                return factories.Keys.Where(k => k.Type == typeof (T)).Select(key => Resolve<T>(key.Name));
            }
        }

        private RegistrationKey MakeKey<T>(string name)
        {
            return new RegistrationKey(name, typeof (T));
        }

        private bool TryResolve(RegistrationKey key, Container c, out object result)
        {
            Func<Container, string, Type, object> factory;
            lock (factoriesLock)
            {
                bool found = factories.TryGetValue(key, out factory);
                if (!found)
                {
                    result = null;
                    return false;
                }
            }
            result = factory(c, key.Name, key.Type);
            return true;
        }
    }
}
