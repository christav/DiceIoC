using System;
using System.Linq.Expressions;

namespace DiceIoC.Catalogs
{
    public interface IRegistrar
    {
        IRegistrar Register(Type serviceType, string name,
            Expression<Func<Container, object>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers);

        IRegistrar Register<TService>(string name, Expression<Func<Container, TService>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers);

        IRegistrar Register<TService>(Expression<Func<Container, TService>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers);
    }
}
