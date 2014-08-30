using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DiceIoC.Registrations
{
    using FactoryExpression = Expression<Func<Container, object>>;
    using FactoryModifier = Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>;

    public abstract class RegistrationBase
    {
        protected readonly FactoryExpression FactoryExpression;

        protected RegistrationBase(FactoryExpression factoryExpression)
        {
            FactoryExpression = factoryExpression;
        }

        protected RegistrationBase(FactoryExpression factoryExpression, IEnumerable<FactoryModifier> modifiers)
        {
            FactoryExpression = ApplyModifiers(factoryExpression, modifiers);
        }

        protected Expression<Func<Container, object>> ApplyModifiers(FactoryExpression factoryExpression,
            IEnumerable<FactoryModifier> modifiers)
        {
            return modifiers.Aggregate(factoryExpression, (factory, modifier) => modifier(factory));
        }
    }
}
