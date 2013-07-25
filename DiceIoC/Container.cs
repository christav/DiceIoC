using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceIoC
{
    public class Container
    {
        private readonly Dictionary<KeyValuePair<string, Type>, Func<Container, string, Type, object>> factories =
            new Dictionary<KeyValuePair<string, Type>, Func<Container, string, Type, object>>();

        private readonly Container parent;

        public Container(Container parent = null)
        {
            this.parent = parent;
        }

        public Container Register<T>(string name, Func<Container, string, Type, T> factory)
        {
            var key = MakeKey<T>(name);
            factories[key] = (c, n, t) => (object)factory(c, n, t);
            return this;
        }

        public Container Register<T>(Func<Container, string, Type, T> factory)
        {
            return Register(null, factory);
        }

        public Container Register<T>(string name, Func<Container, T> factory)
        {
            return Register(name, (c, n, t) => factory(c));
        }

        public Container Register<T>(Func<Container, T> factory)
        {
            return Register(null, factory);
        }

        public T Resolve<T>(string name)
        {
            T resolved;
            if (!TryResolve(name, out resolved))
            {
                throw new ArgumentException(string.Format("The name/type {0}/{1} is not registered", name,
                    typeof(T).Name));
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
            return factories.Keys.Where(k => k.Value == typeof (T)).Select(key => Resolve<T>(key.Key));
        }

        public bool IsRegistered<T>(string name)
        {
            var key = MakeKey<T>(name);
            return factories.ContainsKey(key);
        }

        public IEnumerable<KeyValuePair<string, Type>> Registrations
        {
            get { return factories.Keys; }
        }

        public static Func<Container, string, Type, T> Singleton<T>(Func<Container, string, Type, T> factory)
        {
            bool created = false;
            T cache = default(T);
            return (ioc, name, t) => {
                if (!created)
                {
                    cache = factory(ioc, name, t);
                    created = true;
                }
                return cache;
            };
        }

        public static Func<Container, string, Type, T> Singleton<T>(Func<Container, T> factory)
        {
            return Singleton((ioc, n, t) => factory(ioc));
        }

        private KeyValuePair<string, Type> MakeKey<T>(string name)
        {
            return new KeyValuePair<string, Type>(name, typeof (T));
        }

        private bool TryResolve(KeyValuePair<string, Type> key, Container c, out object result)
        {
            Func<Container, string, Type, object> factory;
            if (factories.TryGetValue(key, out factory))
            {
                result = factory(c, key.Key, key.Value);
                return true;
            }
            if (parent != null)
            {
                return parent.TryResolve(key, c, out result);
            }
            result = null;
            return false;
        }
    }
}
