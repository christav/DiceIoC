using System;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Basics
{
    public class ScopeLifetime
    {
        private IContainer container;

        public ScopeLifetime()
        {
            container = new Catalog()
                .Register(c => new ConcreteClass(), Scope.Lifetime)
                .CreateContainer();
        }

        [Fact]
        public void ContainerByDefaultHasNoScope()
        {
            var c = new Catalog().CreateContainer();
            Assert.Null(c.CurrentScope);
        }

        [Fact]
        public void ResolvingScopedLifetimeObjectsWithNoScopeThrows()
        {
            Assert.Throws<InvalidOperationException>(() => container.Resolve<ConcreteClass>());
        }

        [Fact]
        public void ResolvingScopedLifetimeObjectsWithScopeSucceeds()
        {
            var scope = container.InScope(new LifetimeScope());
            scope.Resolve<ConcreteClass>();
        }

        [Fact]
        public void DisposingScopeDisposesScopedObjects()
        {
            ConcreteClass result;
            using(var scope = new LifetimeScope())
            {
                result = container.InScope(scope).Resolve<ConcreteClass>();
                result.Disposed.Should().BeFalse();
            }
            result.Disposed.Should().BeTrue();
        }

        [Fact]
        public void ResolvingSameScopedTypeReturnsSameObject()
        {
            var scope = new LifetimeScope();
            var r1 = container.InScope(scope).Resolve<ConcreteClass>();
            var r2 = container.InScope(scope).Resolve<ConcreteClass>();

            r1.Should().BeSameAs(r2);
        }

        [Fact]
        public void ResolvingInSeparateScopesGivesSeparateObjects()
        {
            var scope1 = new LifetimeScope();
            var scope2 = new LifetimeScope();
            var r1 = container.InScope(scope1).Resolve<ConcreteClass>();
            var r2 = container.InScope(scope2).Resolve<ConcreteClass>();

            r1.Should().NotBeSameAs(r2);
        }
    }
}
