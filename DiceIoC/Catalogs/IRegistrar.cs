using System;
using System.Linq.Expressions;

namespace DiceIoC.Catalogs
{
    public interface IRegistrar
    {
        IRegistrar Register(Type serviceType, string name,
            Expression<Func<IContainer, object>> factoryExpression,
            params Func<
                Expression<Func<IContainer, object>>,
                Expression<Func<IContainer, object>>
            >[] modifiers);

        IRegistrar Register<TService>(string name, Expression<Func<IContainer, TService>> factoryExpression,
            params Func<
                Expression<Func<IContainer, object>>,
                Expression<Func<IContainer, object>>
            >[] modifiers);

        IRegistrar Register<TService>(Expression<Func<IContainer, TService>> factoryExpression,
            params Func<
                Expression<Func<IContainer, object>>,
                Expression<Func<IContainer, object>>
            >[] modifiers);
    }
}
