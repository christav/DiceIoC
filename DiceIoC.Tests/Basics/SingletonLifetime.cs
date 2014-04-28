﻿using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Basics
{
    public class SingletonLifetime
    {
        [Fact]
        public void CanRegisterWithSingleton()
        {
            var catalog = new Catalog();
            using (var singleton = new LifetimeContainer())
            {
                catalog.Register(c => new ConcreteClass(), singleton);
                Assert.DoesNotThrow(() => catalog.CreateContainer());
            }
        }

        [Fact]
        public void SingletonLifetimeReturnsSameInstance()
        {
            using (var singleton = new LifetimeContainer())
            {
                var container = new Catalog()
                    .Register(c => new ConcreteClass(), singleton)
                    .CreateContainer();

                var o1 = container.Resolve<ConcreteClass>();
                var o2 = container.Resolve<ConcreteClass>();

                o2.Should().BeSameAs(o1);
            }
        }

        [Fact]
        public void DisposingContainerDisposesInstances()
        {
            ConcreteClass o1;

            using (var singleton = new LifetimeContainer())
            {
                var container = new Catalog()
                    .Register(c => new ConcreteClass(), singleton)
                    .CreateContainer();

                o1 = container.Resolve<ConcreteClass>();

                Assert.False(o1.Disposed);
            }

            Assert.True(o1.Disposed);
        }

        [Fact]
        public void ClearOfSingletonDisposesInstances()
        {
            var singleton = new LifetimeContainer();
            var container = new Catalog()
                .Register(c => new ConcreteClass(), singleton)
                .CreateContainer();

            var o1 = container.Resolve<ConcreteClass>();
            singleton.Clear();
            o1.Disposed.Should().BeTrue();

            var o2 = container.Resolve<ConcreteClass>();

            o2.Should().NotBeSameAs(o1);
            o2.Disposed.Should().BeFalse();
        }

        [Fact]
        public void WithSingletonRegistrationGivesSingletons()
        {
            using (var singleton = new LifetimeContainer())
            {
                var container = new Catalog()
// ReSharper disable once AccessToDisposedClosure
                    .With(() => singleton.Lifetime, r => r.Register(c => new ConcreteClass()))
                    .CreateContainer();
                var o1 = container.Resolve<ConcreteClass>();
                var o2 = container.Resolve<ConcreteClass>();

                o1.Should().BeSameAs(o2);
            }
        }
    }
}
