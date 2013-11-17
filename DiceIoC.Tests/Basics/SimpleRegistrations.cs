using System;
using System.Linq;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Basics
{
    public class SimpleRegistrations : IDisposable
    {
        private Container container;

        public SimpleRegistrations()
        {
            container = new Container();
        }

        public void Dispose()
        {
        }

        [Fact]
        public void DefaultContainerHasNoRegistrations()
        {
            container.Registrations.Count().Should().Be(0);
        }

        [Fact]
        public void ResolvingUnregisteredTypeShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => container.Resolve<SimpleRegistrations>());
        }

        [Fact]
        public void ResolvingRegisteredTypeSucceeds()
        {
            container.Register(c => new ConcreteClass());
        }

        [Fact]
        public void ResolvingInvokesRegisteredDelegate()
        {
            bool called = false;
            container.Register(c => new ConcreteClass(),
                Passthrough.Modifier((c, n, t) => { called = true; }));

            container.Resolve<ConcreteClass>();
            called.Should().BeTrue();
        }

        [Fact]
        public void ContainerPassedToDelegateIsResolvingContainer()
        {
            Container passedContainer = null;
            container.Register(c => new ConcreteClass(),
                Passthrough.Modifier((c, n, t) => {
                    passedContainer = c;
                }));

            container.Resolve<ConcreteClass>();
            passedContainer.Should().BeSameAs(container);
        }

        [Fact]
        public void CanRegisterInterface()
        {
            container.Register<ISimpleInterface>(c => new SimpleInterfaceImpl());

            container.Resolve<ISimpleInterface>().Should().BeOfType<SimpleInterfaceImpl>();
        }
    }
}
