using System.Collections.Generic;
using System.Linq;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Basics
{
    public class ResolvePermutationTests
    {
        private readonly Container container;

        public ResolvePermutationTests()
        {
            container = new Catalog()
                .Register(c => new ConcreteClass())
                .Register("other", c => new ConcreteClass())
                .CreateContainer();
        }

        [Fact]
        public void PublicContainersHaveNoPerResolveObjects()
        {
            container.PerResolveObjects.Should().BeNull();
        }

        [Fact]
        public void CanResolveByName()
        {
            Assert.DoesNotThrow(() => container.Resolve<ConcreteClass>("other"));
        }

        [Fact]
        public void CanResolveLooselyTyped()
        {
            object o = container.Resolve(typeof (ConcreteClass));
            o.Should().BeOfType<ConcreteClass>();
        }

        [Fact]
        public void CanResolveLooselyTypedByName()
        {
            object o = container.Resolve(typeof (ConcreteClass), "other");
            o.Should().BeOfType<ConcreteClass>();
        }

        [Fact]
        public void CanResolveAllStronglyTyped()
        {
            List<ConcreteClass> results = container.ResolveAll<ConcreteClass>().ToList();
            results.Should().HaveCount(2);
        }

        [Fact]
        public void CanResolveAllLooselyTyped()
        {
            List<object> results = container.ResolveAll(typeof (ConcreteClass)).ToList();
            results.Should().HaveCount(2);
            results.Cast<ConcreteClass>().Should().HaveCount(2);
        }

        [Fact]
        public void TryResolveByTypeSucceeds()
        {
            ConcreteClass cc;
            container.TryResolve(out cc).Should().BeTrue();
            cc.Should().NotBeNull();
        }

        [Fact]
        public void TryResolveByTypeAndNameSucceeds()
        {
            ConcreteClass cc;
            container.TryResolve("other", out cc).Should().BeTrue();
            cc.Should().NotBeNull();
        }

        [Fact]
        public void TryResolveForUnregisteredNameShouldReturnFalse()
        {
            ConcreteClass cc;
            container.TryResolve("notregistered", out cc).Should().BeFalse();
        }

        [Fact]
        public void TryResolveForUnregisteredTypeShouldReturnFalse()
        {
            ISimpleInterface si;
            container.TryResolve(out si).Should().BeFalse();
        }

        [Fact]
        public void TryResolveLooselyTypedShouldSucceed()
        {
            object result;
            container.TryResolve(typeof (ConcreteClass), out result).Should().BeTrue();
            result.Should().BeOfType<ConcreteClass>();
        }

        [Fact]
        public void TryResolveLooselyTypedByNameShouldSucceed()
        {
            object result;
            container.TryResolve(typeof(ConcreteClass), "other", out result).Should().BeTrue();
            result.Should().BeOfType<ConcreteClass>();
        }

        [Fact]
        public void TryResolveLooselyTypedForUnregisteredNameShouldReturnFalse()
        {
            object result;
            container.TryResolve(typeof(ConcreteClass), "notregistered", out result).Should().BeFalse();
        }

        [Fact]
        public void TryResolveLooselyTypedForUnregisteredTypeShouldReturnFalse()
        {
            object result;
            container.TryResolve(typeof(ISimpleInterface), out result).Should().BeFalse();
        }


    
    }
}
