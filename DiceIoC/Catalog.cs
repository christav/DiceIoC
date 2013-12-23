using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DiceIoC
{
    public class Catalog
    {
        private readonly Dictionary<RegistrationKey, Expression<Func<Container, object>>> factories =
            new Dictionary<RegistrationKey, Expression<Func<Container, object>>>();

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

        private Catalog RegisterFactory(Type serviceType, string name, Expression<Func<Container, object>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            var key = new RegistrationKey(serviceType, name);
            factories[key] = modifiers.Aggregate(factoryExpression, (current, modifier) => modifier(current));
            return this;
        }

        public Catalog Register<T>(string name, Expression<Func<Container, T>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            return RegisterFactory(typeof(T), name, CastToObject(factoryExpression), modifiers);
        }

        public Catalog Register<T>(Expression<Func<Container, T>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            return RegisterFactory(typeof(T), null, CastToObject(factoryExpression), modifiers);
        }

        public Container CreateContainer()
        {
            return new Container(this, GetFactories());
        }

        private Dictionary<RegistrationKey, Func<Container, object>> GetFactories()
        {
            var result = new Dictionary<RegistrationKey, Func<Container, object>>();
            return GetFactories(result);
        }

        private Dictionary<RegistrationKey, Func<Container, object>> GetFactories(
            Dictionary<RegistrationKey, Func<Container, object>> factories)
        {
            if (parentCatalog != null)
            {
                parentCatalog.GetFactories(factories);
            }

            foreach (var item in this.factories)
            {
                var optimized = (Expression<Func<Container, object>>)(new ResolveCallInliningVisitor(this.factories).Visit(item.Value));
                factories[item.Key] = optimized.Compile();
            }
            return factories;
        }

        private Expression<Func<Container, object>> CastToObject<T>(
            Expression<Func<Container, T>> originalExpression)
        {
            var c = Expression.Parameter(typeof(Container), "c");

            var cast = Expression.Convert(
                Expression.Invoke(originalExpression, c), typeof(object));

            return Expression.Lambda<Func<Container, object>>(
                cast, c);
        }
    }
}
