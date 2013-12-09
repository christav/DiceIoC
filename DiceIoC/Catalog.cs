using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DiceIoC
{
    public class Catalog
    {
        private readonly Dictionary<RegistrationKey, Expression<Func<Container, string, Type, object>>> factories =
            new Dictionary<RegistrationKey, Expression<Func<Container, string, Type, object>>>();

        private readonly Catalog parentCatalog;

        public Catalog()
        {
            
        }

        public Catalog(Catalog parentCatalog)
        {
            this.parentCatalog = parentCatalog;
        }

        public IEnumerable<KeyValuePair<string, Type>> Registrations
        {
            get { return factories.Keys.Select(k => new KeyValuePair<string, Type>(k.Name, k.Type)); }
        }

        public Catalog Register<T>(string name, Expression<Func<Container, string, Type, T>> factoryExpression,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
        {
            RegistrationKey key = MakeKey<T>(name);
            var objFactory = CastToObject(factoryExpression);
            factories[key] = modifiers.Aggregate(objFactory, (current, modifier) => modifier(current));
            return this;
        }

        public Catalog Register<T>(Expression<Func<Container, string, Type, T>> factoryExpression,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
        {
            return Register(null, factoryExpression, modifiers);
        }

        public Catalog Register<T>(string name, Expression<Func<Container, T>> factoryExpression,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
        {
            return Register(name, ConvertExpression(factoryExpression), modifiers);
        }

        public Catalog Register<T>(Expression<Func<Container, T>> factory,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
        {
            return Register(null, factory, modifiers);
        }

        public Container CreateContainer()
        {
            return new Container(this, GetFactories());
        }

        private Dictionary<RegistrationKey, Func<Container, string, Type, object>> GetFactories()
        {
            var result = new Dictionary<RegistrationKey, Func<Container, string, Type, object>>();
            return GetFactories(result);
        }

        private Dictionary<RegistrationKey, Func<Container, string, Type, object>> GetFactories(
            Dictionary<RegistrationKey, Func<Container, string, Type, object>> factories)
        {
            if (parentCatalog != null)
            {
                parentCatalog.GetFactories(factories);
            }

            foreach (var item in this.factories)
            {
                factories[item.Key] = item.Value.Compile();
            }
            return factories;
        }

        private RegistrationKey MakeKey<T>(string name)
        {
            return new RegistrationKey(name, typeof(T));
        }

        private Expression<Func<Container, String, Type, T>> ConvertExpression<T>(
            Expression<Func<Container, T>> originalExpression)
        {
            var c = Expression.Parameter(typeof(Container), "c");
            var name = Expression.Parameter(typeof(string), "name");
            var type = Expression.Parameter(typeof(Type), "resolvedType");

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
                Expression.Invoke(originalExpression, c, name, type), typeof(object));

            return Expression.Lambda<Func<Container, string, Type, object>>(
                cast, c, name, type);
        }

    }
}
