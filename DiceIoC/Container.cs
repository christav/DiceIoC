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
            return (T) Resolve(MakeKey<T>(name), this);
        }

        public T Resolve<T>()
        {
            return Resolve<T>(null);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return factories.Keys.Where(k => k.Value == typeof (T)).Select(key => (T) Resolve(key, this));
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

        private object Resolve(KeyValuePair<string, Type> key, Container c)
        {
            Func<Container, string, Type, object> factory;
            if (factories.TryGetValue(key, out factory))
            {
                return factory(c, key.Key, key.Value);
            }
            if (parent != null)
            {
                return parent.Resolve(key, c);
            }
            throw new ArgumentException(string.Format("The name/type {0}/{1} is not registered", key.Key, key.Value.Name));
        }
    }
}
