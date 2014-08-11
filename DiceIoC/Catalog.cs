using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DiceIoC.Catalogs;
using DiceIoC.Containers;

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

        public Catalog Register<T>(string name, Expression<Func<Container, T>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            return Register(typeof(T), name, CatalogBase.CastToObject(factoryExpression), modifiers);
        }

        public Catalog Register<T>(Expression<Func<Container, T>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            return Register(typeof (T), null, CatalogBase.CastToObject(factoryExpression), modifiers);
        }

        public Catalog With(Func<Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>> modifierFactory,
            Action<IRegistrar> registrations)
        {
            var registrar = new WithModifierRegistrar(modifierFactory);
            registrations(registrar);
            registrar.Register(this);
            return this;
        }

        public Container CreateContainer()
        {
            return new ConfiguredContainer(this);
        }

        IEnumerable<KeyValuePair<RegistrationKey, Expression<Func<Container, object>>>> ICatalog.GetFactoryExpressions()
        {
            return innerCatalog.GetFactoryExpressions();
        }

        IEnumerable<Expression<Func<Container, object>>> ICatalog.GetFactoryExpressions(Type serviceType)
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
