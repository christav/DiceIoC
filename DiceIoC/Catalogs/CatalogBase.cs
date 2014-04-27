using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DiceIoC.Catalogs
{
    public abstract class CatalogBase : IRegistrar
    {
        public abstract IRegistrar Register(Type serviceType, string name,
            Expression<Func<IContainer, object>> factoryExpression,
            params Func<
                Expression<Func<IContainer, object>>,
                Expression<Func<IContainer, object>>
                >[]  modifiers);

        public IRegistrar Register(Type serviceType, Expression<Func<IContainer, object>> factoryExpression,
            params Func<
                Expression<Func<IContainer, object>>,
                Expression<Func<IContainer, object>>
                >[] modifiers)
        {
            return Register(serviceType, null, factoryExpression, null);
        }

        public IRegistrar Register<TService>(string name, Expression<Func<IContainer, TService>> factoryExpression,
            params Func<
                Expression<Func<IContainer, object>>,
                Expression<Func<IContainer, object>>
                >[] modifiers)
        {
            return Register(typeof (TService), name, CastToObject(factoryExpression), modifiers);
        }

        public IRegistrar Register<TService>(Expression<Func<IContainer, TService>> factoryExpression,
            params Func<
                Expression<Func<IContainer, object>>,
                Expression<Func<IContainer, object>>
                >[] modifiers)
        {
            return Register(typeof(TService), null, CastToObject(factoryExpression), modifiers);
        }

        protected Expression<Func<IContainer, object>> ApplyModifiers(
            Expression<Func<IContainer, object>> factoryExpression,
            IEnumerable<Func<Expression<Func<IContainer, object>>, Expression<Func<IContainer, object>>>> modifiers)
        {
            return modifiers.Aggregate(factoryExpression, (factory, modifier) => modifier(factory));
        }

        internal static Expression<Func<IContainer, object>> CastToObject<T>(
            Expression<Func<IContainer, T>> originalExpression)
        {
            var c = Expression.Parameter(typeof(IContainer), "c");

            var cast = Expression.Convert(
                Expression.Invoke(originalExpression, c), typeof(object));

            return Expression.Lambda<Func<IContainer, object>>(
                cast, c);
        }
    }
}
