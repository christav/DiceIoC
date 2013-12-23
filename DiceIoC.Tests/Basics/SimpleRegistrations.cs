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
            container.Should().NotBeNull();
        }
        [Fact]
        public void ContainerPointsToCatalog()
        {
            var container = catalog.CreateContainer();
            container.Catalog.Should().BeSameAs(catalog);
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
                Passthrough.Modifier(c => { called = true; }));

            var container = catalog.CreateContainer();
            container.Resolve<ConcreteClass>();
            called.Should().BeTrue();
        }

        [Fact]
        public void ContainerPassedToDelegateIsResolvingContainer()
        {
            Container passedContainer = null;
            catalog.Register(c => new ConcreteClass(),
                Passthrough.Modifier(c => {
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

        [Fact]
        public void CanRegisterWithName()
        {
            catalog.Register<ISimpleInterface>("a name", c => new SimpleInterfaceImpl());
            catalog.Register<ConcreteClassWithDependencies>("another name",
                c => new ConcreteClassWithDependencies(c.Resolve<ISimpleInterface>("a name")));
            var container = catalog.CreateContainer();
            container.Resolve<ConcreteClassWithDependencies>("another name").Should().BeOfType<ConcreteClassWithDependencies>();
        }

        [Fact]
        public void CanRegisterWithDynamicName()
        {
            catalog.Register<ISimpleInterface>("a", c => new SimpleInterfaceImpl());

            Func<string> getName = () => "a";

            catalog.Register<ConcreteClassWithDependencies>("another name",
                c => new ConcreteClassWithDependencies(c.Resolve<ISimpleInterface>(getName())));
            var container = catalog.CreateContainer();
            container.Resolve<ConcreteClassWithDependencies>("another name").Should().BeOfType<ConcreteClassWithDependencies>();
        }
    }
}
