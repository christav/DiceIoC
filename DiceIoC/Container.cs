using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

        // TODO: do modifiers as expression rewriters instead of straight funcs
        public Container Register<T>(string name, Expression<Func<Container, string, Type, T>> factoryExpression,
            params Func<
                Expression<Func<Container, string, Type, object>>, 
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
        {
            RegistrationKey key = MakeKey<T>(name);
            var objFactory = CastToObject(factoryExpression);
            factories[key] = modifiers.Aggregate(objFactory, (current, modifier) => modifier(current)).Compile();
            return this;
        }

        public Container Register<T>(Expression<Func<Container, string, Type, T>> factoryExpression,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
        {
            return Register(null, factoryExpression, modifiers);
        }

        public Container Register<T>(string name, Expression<Func<Container, T>> factoryExpression,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
        {
            return Register(name, ConvertExpression(factoryExpression), modifiers);
        }

        public Container Register<T>(Expression<Func<Container, T>> factory,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
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

        private Expression<Func<Container, String, Type, T>> ConvertExpression<T>(
            Expression<Func<Container, T>> originalExpression)
        {
            var c = Expression.Parameter(typeof (Container), "c");
            var name = Expression.Parameter(typeof (string), "name");
            var type = Expression.Parameter(typeof (Type), "resolvedType");

            return Expression.Lambda<Func<Container, string, Type, T>>(
                Expression.Invoke(originalExpression, c),
                c, name, type);
        }

        private Expression<Func<Container, string, Type, Object>> CastToObject<T>(
            Expression<Func<Container, string, Type, T>> originalExpression)
        {
            var c = Expression.Parameter(typeof(Container), "c");
            var name = Expression.Parameter(typeof(string), "name");
            var type = Expression.Parameter(typeof(Type), "resolvedType");

            var cast = Expression.Convert(
                Expression.Invoke(originalExpression, c, name, type), typeof (object));

            return Expression.Lambda<Func<Container, string, Type, object>>(
                cast, c, name, type);
        }
    }
}