using System;
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

    public class DirectRegistrationTests
    {
        [Fact]
        public void DirectRegistrationReturnsExpressionByDefault()
        {
            FactoryExpression expr = c => new ConcreteClass();
            var reg = new DirectRegistration(expr, Enumerable.Empty<FactoryModifier>());

            reg.GetFactory().Should().BeSameAs(expr);
        }

        [Fact]
        public void ModifiersAreAppliedToFactoryExpression()
        {
            var modifier = new ModificationTracker();
            FactoryExpression expr = c => new ConcreteClass();
            var reg = new DirectRegistration(expr, new FactoryModifier[] { modifier.Modifier });
            reg.GetFactory();

            modifier.ModifierCallCount.Should().Be(1);
            modifier.LastFactoryExpression.Should().BeSameAs(expr);
        }

        [Fact]
        public void OnDemandFactoriesAreNotReturned()
        {
            FactoryExpression expr = c => new ConcreteClass();
            var reg = new DirectRegistration(expr, Enumerable.Empty<FactoryModifier>());

            reg.GetFactory(typeof (ConcreteClass)).Should().BeNull();
            reg.GetFactory(typeof (string)).Should().BeNull();
        }
    }
}
