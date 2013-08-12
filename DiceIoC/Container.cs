using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceIoC
{
    public class Container
    {
        private readonly Dictionary<RegistrationKey, Func<Container, string, Type, object>> factories =
            new Dictionary<RegistrationKey, Func<Container, string, Type, object>>();

        private readonly Container parent;

        public Container(Container parent = null)
        {
            this.parent = parent;
        }

        public IEnumerable<KeyValuePair<string, Type>> Registrations
        {
            get { return factories.Keys.Select(k => new KeyValuePair<string, Type>(k.Name, k.Type)); }
        }

        public Container Register<T>(string name, Func<Container, string, Type, T> factory,
                                     params Func<Func<Container, string, Type, object>, Func<Container, string, Type, object>>[]
                                         modifiers)
        {
            RegistrationKey key = MakeKey<T>(name);
            Func<Container, string, Type, object> objFactory = (c, n, t) => (object) factory(c, n, t);
            factories[key] = modifiers.Aggregate(objFactory, (current, modifier) => modifier(current));
            return this;
        }

        public Container Register<T>(Func<Container, string, Type, T> factory,
                                     params Func<Func<Container, string, Type, object>, Func<Container, string, Type, object>>[]
                                         modifiers)
        {
            return Register(null, factory, modifiers);
        }

        public Container Register<T>(string name, Func<Container, T> factory,
                                     params Func<Func<Container, string, Type, object>, Func<Container, string, Type, object>>[]
                                         modifiers)
        {
            return Register(name, (c, n, t) => factory(c), modifiers);
        }

        public Container Register<T>(Func<Container, T> factory,
                                     params Func<Func<Container, string, Type, object>, Func<Container, string, Type, object>>[]
                                         modifiers)
        {
            return Register(null, factory, modifiers);
        }

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
            return factories.Keys.Where(k => k.Type == typeof (T)).Select(key => Resolve<T>(key.Name));
        }

        public bool IsRegistered<T>(string name)
        {
            RegistrationKey key = MakeKey<T>(name);
            return factories.ContainsKey(key);
        }

        private RegistrationKey MakeKey<T>(string name)
        {
            return new RegistrationKey(name, typeof (T));
        }

        private bool TryResolve(RegistrationKey key, Container c, out object result)
        {
            Func<Container, string, Type, object> factory;
            if (factories.TryGetValue(key, out factory))
            {
                result = factory(c, key.Name, key.Type);
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