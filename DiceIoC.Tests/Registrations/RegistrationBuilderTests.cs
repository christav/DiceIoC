using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DiceIoC.Registrations;
using DiceIoC.Tests.SampleTypes;
using DiceIoC.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Registrations
{
    using FactoryExpression = Expression<Func<Container, object>>;
    using FactoryModifier = Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>;

    public class RegistrationBuilderTests
    {
        private readonly FactoryExpression concreteExpr = c => new ConcreteClass();
        private readonly FactoryExpression genericExpr = c => new OneTypeArgGenericImpl<T0>();
        private readonly IEnumerable<FactoryModifier> noModifiers = Enumerable.Empty<FactoryModifier>().ToArray();

        [Fact]
        public void RegistrationOfConcreteTypeCreatesDirectRegistration()
        {
            var reg = RegistrationBuilder.CreateRegistration(typeof (ConcreteClass), concreteExpr, noModifiers);

            reg.Should().BeOfType<DirectRegistration>();
        }

        [Fact]
        public void RegistrationOfOpenGenericTypeCreatesOpenGenericRegistration()
        {
            RegistrationBuilder.CreateRegistration(typeof (IOneTypeArgGenericInterface<T0>), genericExpr, noModifiers)
                .Should().BeOfType<OpenGenericRegistration>();
        }

        [Fact]
        public void ModifiersArePassedThroughToRegistration()
        {
            var modifier = new ModificationTracker();

            var reg = RegistrationBuilder.CreateRegistration(typeof (ConcreteClass), concreteExpr,
                new FactoryModifier[] {modifier.Modifier});
            reg.GetFactory();

            modifier.ModifierCallCount.Should().Be(1);
            modifier.LastFactoryExpression.Should().Be(concreteExpr);
        }

        [Fact]
        public void InvalidGenericRegistrationThrows()
        {
            Assert.Throws<InvalidOperationException>(
                () => RegistrationBuilder.CreateRegistration(typeof (IOneTypeArgGenericInterface<T1>), genericExpr,
                noModifiers));
        }
    }
}
