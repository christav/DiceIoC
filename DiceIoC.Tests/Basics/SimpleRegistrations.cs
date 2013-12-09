using System;
using System.Linq;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Basics
{
    public class SimpleRegistrations : IDisposable
    {
        private Catalog catalog;

        public SimpleRegistrations()
        {
            catalog = new Catalog();
        }

        public void Dispose()
        {
        }

        [Fact]
        public void DefaultContainerHasNoRegistrations()
        {
            catalog.Registrations.Count().Should().Be(0);
        }

        [Fact]
        public void CanCreateContainerFromCatalog()
        {
            var container = catalog.CreateContainer();
            Assert.NotNull(container);
        }
        [Fact]
        public void ContainerPointsToCatalog()
        {
            var container = catalog.CreateContainer();
            Assert.Same(catalog, container.Catalog);
        }

        [Fact]
        public void ResolvingUnregisteredTypeShouldThrow()
        {
            var container = catalog.CreateContainer();
            Assert.Throws<ArgumentException>(() => container.Resolve<SimpleRegistrations>());
        }

        [Fact]
        public void ResolvingRegisteredTypeSucceeds()
        {
            catalog.Register(c => new ConcreteClass());
        }

        [Fact]
        public void ResolvingInvokesRegisteredDelegate()
        {
            bool called = false;
            catalog.Register(c => new ConcreteClass(),
                Passthrough.Modifier((c, n, t) => { called = true; }));

            var container = catalog.CreateContainer();
            container.Resolve<ConcreteClass>();
            called.Should().BeTrue();
        }

        [Fact]
        public void ContainerPassedToDelegateIsResolvingContainer()
        {
            Container passedContainer = null;
            catalog.Register(c => new ConcreteClass(),
                Passthrough.Modifier((c, n, t) => {
                    passedContainer = c;
                }));

            var container = catalog.CreateContainer();
            container.Resolve<ConcreteClass>();
            passedContainer.Should().BeSameAs(container);
        }

        [Fact]
        public void CanRegisterInterface()
        {
            catalog.Register<ISimpleInterface>(c => new SimpleInterfaceImpl());
            var container = catalog.CreateContainer();
            container.Resolve<ISimpleInterface>().Should().BeOfType<SimpleInterfaceImpl>();
        }
    }
}
