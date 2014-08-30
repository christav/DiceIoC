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

    public class OpenGenericRegistrationTests
    {
        [Fact]
        public void OpenGenericRegistrationReturnsNullByDefault()
        {
            FactoryExpression expr = c => new OneTypeArgGenericImpl<T0>();
            var reg = new OpenGenericRegistration(typeof (OneTypeArgGenericImpl<T0>), expr, Enumerable.Empty<FactoryModifier>());

            reg.GetFactory().Should().BeNull();
        }

        [Fact]
        public void MatchingExpressionGetsReturned()
        {
            FactoryExpression expr = c => new OneTypeArgGenericImpl<T0>();
            var reg = new OpenGenericRegistration(typeof(IOneTypeArgGenericInterface<T0>), expr, Enumerable.Empty<FactoryModifier>());

            var ex = reg.GetFactory(typeof (IOneTypeArgGenericInterface<string>));
            ex.Should().NotBeNull();
            ex.Should().NotBeSameAs(expr);
        }

        [Fact]
        public void DifferentMatchingTypesShouldReturnDifferentExpressions()
        {
            FactoryExpression expr = c => new OneTypeArgGenericImpl<T0>();
            var reg = new OpenGenericRegistration(typeof(IOneTypeArgGenericInterface<T0>), expr, Enumerable.Empty<FactoryModifier>());

            var ex1 = reg.GetFactory(typeof(IOneTypeArgGenericInterface<string>));
            var ex2 = reg.GetFactory(typeof (IOneTypeArgGenericInterface<ConcreteClass>));
            ex1.Should().NotBeSameAs(ex2);
        }

        
        [Fact]
        public void NonMatchingTypesReturnNull()
        {
            FactoryExpression expr = c => new OneTypeArgGenericImpl<T0>();
            var reg = new OpenGenericRegistration(typeof(IOneTypeArgGenericInterface<T0>), expr, Enumerable.Empty<FactoryModifier>());

            reg.GetFactory(typeof (ITwoTypeArgGenericInterface<T0, T1>)).Should().BeNull();
        }

        [Fact]
        public void GettingFactoryForBaseTypeOfResolvedTypeReturnsNull()
        {
            FactoryExpression expr = c => new OneTypeArgGenericImpl<T0>();
            var reg = new OpenGenericRegistration(typeof (OneTypeArgGenericImpl<T0>), expr,
                Enumerable.Empty<FactoryModifier>());
            reg.GetFactory(typeof (IOneTypeArgGenericInterface<string>)).Should().BeNull();
        }

        [Fact]
        public void MatchingFactoryActuallyConstructsMatchingType()
        {
            FactoryExpression expr = c => new OneTypeArgGenericImpl<T0>();
            var reg = new OpenGenericRegistration(typeof(IOneTypeArgGenericInterface<T0>), expr, Enumerable.Empty<FactoryModifier>());

            var ex = reg.GetFactory(typeof(IOneTypeArgGenericInterface<string>));
            Func<Container, object> factory = ex.Compile();

            object o = factory(null);

            o.Should().BeOfType<OneTypeArgGenericImpl<string>>();
        }

        [Fact]
        public void ModifiersAreAppliedToFactoryExpression()
        {
            var modifier = new ModificationTracker();
            FactoryExpression expr = c => new OneTypeArgGenericImpl<T0>();
            var reg = new OpenGenericRegistration(typeof(IOneTypeArgGenericInterface<T0>), expr, 
                new FactoryModifier [] { modifier.Modifier });

            var ex = reg.GetFactory(typeof (IOneTypeArgGenericInterface<object>));
            modifier.ModifierCallCount.Should().Be(1);
            // Modifier is supplied after the types are converted
            modifier.LastFactoryExpression.Should().NotBeSameAs(expr);

            var factory = ex.Compile();
            object o = factory(null);
            o.Should().BeOfType<OneTypeArgGenericImpl<object>>();
        }
    }
}
