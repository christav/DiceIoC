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

        public Catalog Register(Type serviceType, string name, Expression<Func<IContainer, object>> factoryExpression,
            params Func<
                Expression<Func<IContainer, object>>,
                Expression<Func<IContainer, object>>
            >[] modifiers)
        {
            var key = new RegistrationKey(serviceType, name);
            if (!GenericMarkers.IsMarkedGeneric(key.Type))
            {
                innerCatalog.Register(serviceType, name, factoryExpression, modifiers);
            }
            else
            {
                genericCatalog.Register(serviceType, name, factoryExpression, modifiers);
            }
            return this;
        }

        public Catalog Register<T>(string name, Expression<Func<IContainer, T>> factoryExpression,
            params Func<
                Expression<Func<IContainer, object>>,
                Expression<Func<IContainer, object>>
            >[] modifiers)
        {
            return Register(typeof(T), name, CatalogBase.CastToObject(factoryExpression), modifiers);
        }

        public Catalog Register<T>(Expression<Func<IContainer, T>> factoryExpression,
            params Func<
                Expression<Func<IContainer, object>>,
                Expression<Func<IContainer, object>>
            >[] modifiers)
        {
            return Register(typeof (T), null, CatalogBase.CastToObject(factoryExpression), modifiers);
        }

        public Container CreateContainer()
        {
            return new Container(this);
        }

        IEnumerable<KeyValuePair<RegistrationKey, Expression<Func<IContainer, object>>>> ICatalog.GetFactoryExpressions()
        {
            return innerCatalog.GetFactoryExpressions();
        }

        IEnumerable<Expression<Func<IContainer, object>>> ICatalog.GetFactoryExpressions(Type serviceType)
        {
            return
                innerCatalog.GetFactoryExpressions(serviceType)
                    .Concat(genericCatalog.GetFactoryExpressions(serviceType));
        }

        Expression<Func<IContainer, object>> ICatalog.GetFactoryExpression(RegistrationKey key)
        {
            return innerCatalog.GetFactoryExpression(key) ?? genericCatalog.GetFactoryExpression(key);
        }
    }
}
