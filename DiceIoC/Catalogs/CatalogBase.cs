using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DiceIoC.Catalogs
{
    public abstract class CatalogBase : IRegistrar
    {
        public abstract IRegistrar Register(Type serviceType, string name,
            Expression<Func<Container, object>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
                >[]  modifiers);

        public IRegistrar Register(Type serviceType, Expression<Func<Container, object>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
                >[] modifiers)
        {
            return Register(serviceType, null, factoryExpression, null);
        }

        public IRegistrar Register<TService>(string name, Expression<Func<Container, TService>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
                >[] modifiers)
        {
            return Register(typeof (TService), name, CastToObject(factoryExpression), modifiers);
        }

        public IRegistrar Register<TService>(Expression<Func<Container, TService>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
                >[] modifiers)
        {
            return Register(typeof(TService), null, CastToObject(factoryExpression), modifiers);
        }

        protected Expression<Func<Container, object>> ApplyModifiers(
            Expression<Func<Container, object>> factoryExpression,
            IEnumerable<Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>> modifiers)
        {
            return modifiers.Aggregate(factoryExpression, (factory, modifier) => modifier(factory));
        }

        internal static Expression<Func<Container, object>> CastToObject<T>(
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
