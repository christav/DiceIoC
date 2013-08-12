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
            var container = new Container();
            container.Register(c => new ConcreteClass(), Singleton.Lifetime());
        }

        [Fact]
        public void SingletonLifetimeReturnsSameInstance()
        {
            var container = new Container();
            container.Register(c => new ConcreteClass(), Singleton.Lifetime());

            var o1 = container.Resolve<ConcreteClass>();
            var o2 = container.Resolve<ConcreteClass>();

            o2.Should().BeSameAs(o1);
        }
    }
}
