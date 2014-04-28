using System;
using DiceIoC.Catalogs;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;

namespace DiceIoC.Tests.Generics
{
    public class ValidGenericRegistrations
    {
        [Fact]
        public void ClosedGenericIsNotValidOpenGenericRegistration()
        {
            GenericMarkers
                .IsValidMarkedGeneric(typeof (ITwoTypeArgGenericInterface<ConcreteClass, ConcreteClass>))
                .Should().BeFalse();
        }

        [Fact]
        public void OpenGenericIsValidOpenGenericRegistration()
        {
            GenericMarkers
                .IsValidMarkedGeneric(typeof (IOneTypeArgGenericInterface<T0>))
                .Should().BeTrue();
        }

        [Theory]
        [InlineData(typeof(IOneTypeArgGenericInterface<T1>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<T1, T0>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<object, T0>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<T1, object>))]
        public void OpenGenericWithWrongMarkerIsInvalid(Type t)
        {
            GenericMarkers.IsValidMarkedGeneric(t).Should().BeFalse();
        }
    }
}
