using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DiceIoC.Registrations
{
    using FactoryExpression = Expression<Func<Container, object>>;
    using FactoryModifier = Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>;

    /// <summary>
    /// A registration object that handles direct registrations
    /// of concrete types, returning the given expression directly
    /// and with no per-type lookup.
    /// </summary>
    public class DirectRegistration : RegistrationBase, IRegistration
    {
        public DirectRegistration(FactoryExpression expression, IEnumerable<FactoryModifier> modifiers)
            : base(expression, modifiers)
        {
        }

        public FactoryExpression GetFactory()
        {
            return FactoryExpression;
        }

        public FactoryExpression GetFactory(Type serviceType)
        {
            return null;
        }
    }
}
