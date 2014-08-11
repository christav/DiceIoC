using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DiceIoC.Catalogs
{
    internal class WithModifierRegistrar : CatalogBase
    {
        private readonly Func<Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>> defaultModifierFactory;

        private readonly List<Tuple<
                Type,
                string, 
                Expression<Func<Container, object>>,
                Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>[]>>
            registrations = new List<Tuple<Type, string, Expression<Func<Container, object>>, Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>[]>>();

        internal WithModifierRegistrar(
            Func<Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>> defaultModifierFactory)
        {
            this.defaultModifierFactory = defaultModifierFactory;
        }

        public override IRegistrar Register(Type serviceType, string name, Expression<Func<Container, object>> factoryExpression, params Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>[] modifiers)
        {
            registrations.Add(Tuple.Create(serviceType, name, factoryExpression, Enumerable.Repeat(defaultModifierFactory(), 1).Concat(modifiers).ToArray()));
            return this;
        }

        public IRegistrar With(
            Func<Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>> newModifier,
            Action<IRegistrar> registrations)
        {
            var nestedRegistrar = new WithModifierRegistrar(newModifier);
            registrations(nestedRegistrar);

            foreach (var registration in nestedRegistrar.registrations)
            {
                this.registrations.Add(Tuple.Create(
                    registration.Item1, 
                    registration.Item2, 
                    registration.Item3,
                    Enumerable.Repeat(defaultModifierFactory(), 1).Concat(registration.Item4).ToArray()));
            }
            return this;
        }

        internal void Register(Catalog catalog)
        {
            foreach (var registration in registrations)
            {
                catalog.Register(registration.Item1, registration.Item2, registration.Item3, registration.Item4);
            }
        }
    }
}
