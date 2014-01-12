using System;
using DiceIoC.Catalogs;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;

namespace DiceIoC.Tests.Generics
{
    public class RegistrationDetection
    {

        [Theory]
        [InlineData(typeof(IOneTypeArgGenericInterface<T0>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T1>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T2>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T3>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T4>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T5>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T6>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T7>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T8>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T9>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T10>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T11>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T12>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T13>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T14>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T15>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<T16>))]
        public void OpenGenericTypeRegistrationCanBeDetected(Type typeToRegister)
        {
            GenericMarkers.IsMarkedGeneric(typeToRegister).Should().BeTrue();
        }

        [Fact]
        public void NonGenericRegistrationIsNotGeneric()
        {
            GenericMarkers.IsMarkedGeneric(typeof(ConcreteClass)).Should().BeFalse();
        }

        [Fact]
        public void ClosedGenericRegistrationIsNotDetectedAsOpen()
        {
            GenericMarkers.IsMarkedGeneric(typeof(IOneTypeArgGenericInterface<object>)).Should().BeFalse();
        }

        [Theory]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T0>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T1>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T2>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T3>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T4>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T5>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T6>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T7>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T8>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T9>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T10>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T11>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T12>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T13>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T14>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T15>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, T16>))]
        public void TwoArgOpenGenericCanBeDetected(Type typeToRegister)
        {
            GenericMarkers.IsMarkedGeneric(typeToRegister).Should().BeTrue();
        }

        [Fact]
        public void MultipleMarkerTypeArgsAreDetected()
        {
            GenericMarkers.IsMarkedGeneric(typeof(ITwoTypeArgGenericInterface<T0, T1>)).Should().BeTrue();
        }

        [Theory]
        [InlineData(typeof(IOneTypeArgGenericInterface<IMarker0>))]
        [InlineData(typeof(IOneTypeArgGenericInterface<IMarker1>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<T0, IMarker1>))]
        [InlineData(typeof(ITwoTypeArgGenericInterface<string, IMarker1>))]
        public void TypesDerivedFromMarkersAreDetectedAsMarkers(Type typeToRegister)
        {
            GenericMarkers.IsMarkedGeneric(typeToRegister).Should().BeTrue();
        }

        private interface IMarker0 : T0 { }

        private interface IMarker1 : T1 { }
    }
}
