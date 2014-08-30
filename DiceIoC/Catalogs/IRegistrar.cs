using System;
using System.Linq.Expressions;

namespace DiceIoC.Catalogs
{
    using FactoryExpression = Expression<Func<Container, object>>;
    using FactoryModifier = Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>;

    public interface IRegistrar
    {
        IRegistrar Register(Type serviceType, string name,
            FactoryExpression factoryExpression,
            params FactoryModifier[] modifiers);

        IRegistrar Register(Type serviceType, FactoryExpression factoryExpression,
            params FactoryModifier[] modifiers);

        IRegistrar Register<TService>(string name, Expression<Func<Container, TService>> factoryExpression,
            params FactoryModifier[] modifiers);

        IRegistrar Register<TService>(Expression<Func<Container, TService>> factoryExpression,
            params FactoryModifier[] modifiers);
    }
}
