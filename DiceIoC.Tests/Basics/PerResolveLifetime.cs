using System;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;

namespace DiceIoC.Tests.Basics
{
    public class PerResolveLifetime
    {
        [Fact]
        public void CanRegisterAndResolveWithPerResolveLifetime()
        {
            var container = new Catalog()
                .Register<ISimpleInterface>(c => new SimpleInterfaceImpl(), PerResolve.Lifetime)
                .CreateContainer();

            ISimpleInterface o = container.Resolve<ISimpleInterface>();

            o.Should().BeOfType<SimpleInterfaceImpl>();
        }

        [Fact]
        public void MultipleResolvesGiveSeparateInstances()
        {
            var container = new Catalog()
                .Register<ISimpleInterface>(c => new SimpleInterfaceImpl(), PerResolve.Lifetime)
                .CreateContainer();

            ISimpleInterface o1 = container.Resolve<ISimpleInterface>();
            ISimpleInterface o2 = container.Resolve<ISimpleInterface>();

            o1.Should().NotBeSameAs(o2);
        }

        [Fact]
        public void SeparatePerResolveDependenciesResolveCorrectly()
        {
            var container = new Catalog()
                .Register(c => new ClassA {B = c.Resolve<ClassB>(), C = c.Resolve<ClassC>(), D = c.Resolve<ClassD>()})
                .Register(c => new ClassC { B = c.Resolve<ClassB>(), D = c.Resolve<ClassD>() })
                .With(() => PerResolve.Lifetime, r => 
                    r.Register(c => new ClassB())
                    .Register(c => new ClassD()))
                .CreateContainer();

            var objA = container.Resolve<ClassA>();
            objA.B.Should().BeSameAs(objA.C.B);
            objA.D.Should().BeSameAs(objA.C.D);
        }

        [Fact]
        public void MutipleResolvesWithMultiplePerResolveDependenciesGiveSeparateObjects()
        {
            var container = new Catalog()
                .Register(c => new ClassA { B = c.Resolve<ClassB>(), C = c.Resolve<ClassC>(), D = c.Resolve<ClassD>() })
                .Register(c => new ClassC { B = c.Resolve<ClassB>(), D = c.Resolve<ClassD>() })
                .With(() => PerResolve.Lifetime, r =>
                    r.Register(c => new ClassB())
                    .Register(c => new ClassD()))
                .CreateContainer();

            var objA1 = container.Resolve<ClassA>();
            var objA2 = container.Resolve<ClassA>();

            objA1.B.Should().BeSameAs(objA1.C.B);
            objA1.D.Should().BeSameAs(objA1.C.D);

            objA2.B.Should().BeSameAs(objA2.C.B);
            objA2.D.Should().BeSameAs(objA2.C.D);

            objA1.B.Should().NotBeSameAs(objA2.C.B);
            objA1.D.Should().NotBeSameAs(objA2.C.D);
        }

        //
        // A simple hierarchy to test out different combinations
        // of per resolve lifetimes.
        //

        // Disable "Never assigned" warnings, they're false and this is
        // just for testing
#pragma warning disable 649

        private class ClassA
        {
            public ClassB B;
            public ClassC C;
            public ClassD D;

        }

        private class ClassB
        {
        }

        private class ClassC
        {
            public ClassB B;
            public ClassD D;
        }

        private class ClassD
        {
            
        }
    }
}
