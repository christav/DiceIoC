using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DiceIoC.Catalogs
{
    using FactoryExpression = Expression<Func<Container, object>>;
    using FactoryModifier = Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>;

    internal class WithModifierRegistrar : IRegistrar
    {
        private readonly Func<FactoryModifier> defaultModifierFactory;

        private readonly List<Tuple<
                Type,
                string, 
                FactoryExpression,
                FactoryModifier[]>>
            registrations = new List<Tuple<Type, string, FactoryExpression, FactoryModifier[]>>();

        internal WithModifierRegistrar(
            Func<FactoryModifier> defaultModifierFactory)
        {
            this.defaultModifierFactory = defaultModifierFactory;
        }

        public IRegistrar Register(Type serviceType, string name, FactoryExpression factoryExpression, params FactoryModifier[] modifiers)
        {
            registrations.Add(Tuple.Create(serviceType, name, factoryExpression, Enumerable.Repeat(defaultModifierFactory(), 1).Concat(modifiers).ToArray()));
            return this;
        }

        public IRegistrar Register(Type serviceType, FactoryExpression factoryExpression, params FactoryModifier[] modifiers)
        {
            return Register(serviceType, null, factoryExpression, modifiers);
        }

        public IRegistrar Register<TService>(string name, Expression<Func<Container, TService>> factoryExpression, params FactoryModifier[] modifiers)
        {
            return Register(typeof (TService), name, Catalog.CastToObject(factoryExpression), modifiers);
        }

        public IRegistrar Register<TService>(Expression<Func<Container, TService>> factoryExpression, params FactoryModifier[] modifiers)
        {
            return Register(typeof (TService), null, Catalog.CastToObject(factoryExpression), modifiers);
        }

        public IRegistrar With(Func<FactoryModifier> newModifier, Action<IRegistrar> registrations)
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
