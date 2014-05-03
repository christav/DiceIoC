using System;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Basics
{
    public class ScopeLifetime : IDisposable
    {
        public ScopeLifetime()
        {
        }

        public void Dispose()
        {
        }

        [Fact]
        public void CanRegisterWithScopedLifetimeShouldCompile()
        {
            new Catalog()
                .Register(c => new ConcreteClass(), Scope.Lifetime)
                .CreateContainer();
        }

        [Fact]
        public void ContainerByDefaultHasNoScope()
        {
            var container = new Catalog().CreateContainer();
            Assert.Null(container.CurrentScope);
        }

        [Fact]
        public void ResolvingScopedLifetimeObjectsWithNoScopeThrows()
        {
            var container = new Catalog()
                .Register(c => new ConcreteClass(), Scope.Lifetime)
                .CreateContainer();

            Assert.Throws<InvalidOperationException>(() => container.Resolve<ConcreteClass>());
        }
    }
}
