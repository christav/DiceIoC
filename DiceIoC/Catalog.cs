using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DiceIoC.Catalogs;

namespace DiceIoC
{
    public sealed class Catalog : ICatalog
    {
        private readonly BasicCatalog innerCatalog = new BasicCatalog();

        public Catalog Register(Type serviceType, string name, Expression<Func<Container, object>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            var key = new RegistrationKey(serviceType, name);
            innerCatalog.Register(serviceType, name, factoryExpression, modifiers);
            return this;
        }

        public Catalog Register<TService>(string name, Expression<Func<Container, TService>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            return Register(typeof (TService), name, CatalogBase.CastToObject(factoryExpression), modifiers);
        }

        public Catalog Register<TService>(Expression<Func<Container, TService>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            return Register(typeof(TService), null, CatalogBase.CastToObject(factoryExpression), modifiers);
        }

        public Container CreateContainer()
        {
            return new Container(this);
        }

        IDictionary<RegistrationKey, Expression<Func<Container, object>>> ICatalog.GetFactoryExpressions()
        {
            return innerCatalog.GetFactoryExpressions();
        }

        private IDictionary<RegistrationKey, Func<Container, object>> GetFactories(
            IDictionary<RegistrationKey, Func<Container, object>> resultingFactories)
        {
            return
                innerCatalog.GetFactoryExpressions(serviceType);
        }

        Expression<Func<Container, object>> ICatalog.GetFactoryExpression(RegistrationKey key)
        {
            return innerCatalog.GetFactoryExpression(key);
        }
    }
}
