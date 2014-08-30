using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DiceIoC.Catalogs;

namespace DiceIoC.Registrations
{
    using FactoryExpression = Expression<Func<Container, object>>;
    using FactoryModifier = Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>;

    public class OpenGenericRegistration : RegistrationBase, IRegistration
    {
        private readonly Type registeredType;
        private readonly FactoryModifier[] modifiers;

        public OpenGenericRegistration(Type registeredType,
                FactoryExpression factoryExpression,
                IEnumerable<FactoryModifier> modifiers)
            : base(factoryExpression)
        {
            this.registeredType = registeredType;
            this.modifiers = modifiers.ToArray();
        }

        public FactoryExpression GetFactory()
        {
            return null;
        }

        public FactoryExpression GetFactory(Type serviceType)
        {
            if (!IsPossibleMatch(serviceType))
            {
                return null;
            }

            var visitor = new GenericTypeRewritingVisitor(serviceType.GetGenericArguments());
            var factory = (FactoryExpression) (visitor.Visit(FactoryExpression));
            return ApplyModifiers(factory, modifiers);
        }

        private bool IsPossibleMatch(Type requestedType)
        {
            if (!requestedType.IsGenericType)
            {
                return false;
            }

            Type requestedGeneric = requestedType.GetGenericTypeDefinition();
            Type registeredGeneric = registeredType.GetGenericTypeDefinition();

            if (!requestedGeneric.IsAssignableFrom(registeredGeneric))
            {
                return false;
            }

            Type[] requestedTypeArgs = requestedType.GetGenericArguments();
            return registeredType.GetGenericArguments()
                .Select((p, i) => new {Index = i, ParameterType = p})
                .All(n => n.ParameterType == requestedTypeArgs[n.Index] ||
                          GenericMarkers.IsGenericMarkerType(n.ParameterType, n.Index));
        }
    }
}

// TODO: Add open generic resolves to perf test - I'm curious how it'll work out
