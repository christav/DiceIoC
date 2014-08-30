using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DiceIoC.Catalogs;

namespace DiceIoC.Registrations
{
    using FactoryExpression = Expression<Func<Container, object>>;
    using FactoryModifier = Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>;

    /// <summary>
    /// Class that contains logic to decide which type of registration to
    /// create based on the type being registered.
    /// </summary>
    public static class RegistrationBuilder
    {
        public static IRegistration CreateRegistration(Type registeredType, FactoryExpression factoryExpression,
            IEnumerable<FactoryModifier> modifiers)
        {
            if (GenericMarkers.IsMarkedGeneric(registeredType))
            {
                if (!GenericMarkers.IsValidMarkedGeneric(registeredType))
                {
                    throw new InvalidOperationException("Invalid open generic registration, markers not in order");
                }
                return new OpenGenericRegistration(registeredType, factoryExpression, modifiers);
            }
            return new DirectRegistration(factoryExpression, modifiers);
        }
    }
}
