using System;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Basics
{
    public class SingletonLifetime : IDisposable
    {
        public SingletonLifetime()
        {
        }

        public void Dispose()
        {
        }

        [Fact]
        public void CanRegisterWithSingleton()
        {
            var catalog = new Catalog();
            catalog.Register(c => new ConcreteClass(), Singleton.Lifetime());
            var container = catalog.CreateContainer();
        }

        [Fact]
        public void SingletonLifetimeReturnsSameInstance()
        {
            var container = new Catalog()
                .Register(c => new ConcreteClass(), Singleton.Lifetime())
                .CreateContainer();

            var o1 = container.Resolve<ConcreteClass>();
            var o2 = container.Resolve<ConcreteClass>();

            o2.Should().BeSameAs(o1);
        }
    }
}
