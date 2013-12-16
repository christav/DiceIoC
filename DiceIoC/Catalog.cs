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

        private Catalog RegisterFactory(Type serviceType, string name, Expression<Func<Container, string, Type, object>> factoryExpression,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
        {
            RegistrationKey key = MakeKey(serviceType, name);
            factories[key] = modifiers.Aggregate(factoryExpression, (current, modifier) => modifier(current));
            return this;
        }

        public Catalog Register<T>(string name, Expression<Func<Container, string, Type, T>> factoryExpression,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
                >[] modifiers)
        {
            return RegisterFactory(typeof(T), name, CastToObject(factoryExpression), modifiers);
        }

        public Catalog Register<T>(Expression<Func<Container, string, Type, T>> factoryExpression,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
        {
            return RegisterFactory(typeof(T), null, CastToObject(factoryExpression), modifiers);
        }

        public Catalog Register<T>(string name, Expression<Func<Container, T>> factoryExpression,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
        {
            return RegisterFactory(typeof(T), name, CastToObject(factoryExpression), modifiers);
        }

        public Catalog Register<T>(Expression<Func<Container, T>> factoryExpression,
            params Func<
                Expression<Func<Container, string, Type, object>>,
                Expression<Func<Container, string, Type, object>>
            >[] modifiers)
        {
            return RegisterFactory(typeof(T), null, CastToObject(factoryExpression), modifiers);
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
                var optimized = (Expression<Func<Container, string, Type, object>>)(new ResolveCallInliningVisitor(this.factories).Visit(item.Value));
                factories[item.Key] = optimized.Compile();
            }
            return factories;
        }

        private RegistrationKey MakeKey<T>(string name)
        {
            return new RegistrationKey(name, typeof(T));
        }

        private RegistrationKey MakeKey(Type t, string name)
        {
            return new RegistrationKey(name, t);
        }

        private Expression<Func<Container, string, Type, object>> CastToObject<T>(
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

        private Expression<Func<Container, string, Type, object>> CastToObject<T>(
            Expression<Func<Container, T>> originalExpression)
        {
            var c = Expression.Parameter(typeof(Container), "c");
            var name = Expression.Parameter(typeof(string), "name");
            var type = Expression.Parameter(typeof(Type), "resolvedType");

            var cast = Expression.Convert(
                Expression.Invoke(originalExpression, c), typeof(object));

            return Expression.Lambda<Func<Container, string, Type, object>>(
                cast, c, name, type);
        }
    }
}
