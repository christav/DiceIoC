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
        private readonly OpenGenericCatalog genericCatalog = new OpenGenericCatalog();

        public Catalog Register(Type serviceType, string name, Expression<Func<Container, object>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            var key = new RegistrationKey(serviceType, name);
            if (!key.IsOpenGenericRegistration)
            {
                innerCatalog.Register(serviceType, name, factoryExpression, modifiers);
            }
            else
            {
                genericCatalog.Register(serviceType, name, factoryExpression, modifiers);
            }
            return this;
        }

        public Catalog Register<T>(string name, Expression<Func<Container, T>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            innerCatalog.Register(name, factoryExpression, modifiers);
            return this;
        }

        public Catalog Register<T>(Expression<Func<Container, T>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            innerCatalog.Register(factoryExpression, modifiers);
            return this;
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
                innerCatalog.GetFactoryExpressions(serviceType)
                    .Concat(genericCatalog.GetFactoryExpressions(serviceType));
        }

        Expression<Func<Container, object>> ICatalog.GetFactoryExpression(RegistrationKey key)
        {
            return innerCatalog.GetFactoryExpression(key) ?? genericCatalog.GetFactoryExpression(key);
        }
    }
}
