using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Generics
{
    public class ResolvingOpenGenerics
    {
        [Fact]
        public void CanRegisterAndResolveOpenGenerics()
        {
            var container = new Catalog()
                .Register<IOneTypeArgGenericInterface<T0>>(c => new OneTypeArgGenericImpl<T0>())
                .CreateContainer();

            var result = container.Resolve<IOneTypeArgGenericInterface<string>>();
            result.Should().BeOfType<OneTypeArgGenericImpl<string>>();
        }

        [Fact]
        public void CanRegisterAndResolveTwoTypeArgumentOpenGenerics()
        {
            var container = new Catalog()
                .Register<ITwoTypeArgGenericInterface<T0, T1>>(c => new TwoTypeArgGenericImpl<T0, T1>())
                .CreateContainer();

            var result = container.Resolve<ITwoTypeArgGenericInterface<int, object>>();

            result.Should().BeOfType<TwoTypeArgGenericImpl<int, object>>();
        }

        [Fact]
        public void CanRegisterAndResolveTwoTypeArgGenericWithOneTypeSpecified()
        {
            var container = new Catalog()
                .Register<ITwoTypeArgGenericInterface<int, T1>>(c => new TwoTypeArgGenericImpl<int, T1>())
                .CreateContainer();

            var result = container.Resolve<ITwoTypeArgGenericInterface<int, string>>();

            result.Should().BeOfType<TwoTypeArgGenericImpl<int, string>>();

            var result2 = container.Resolve<ITwoTypeArgGenericInterface<int, object>>();
            result2.Should().BeOfType<TwoTypeArgGenericImpl<int, object>>();
        }

        [Fact]
        public void DifferentPartiallySetTypeParametersCanBeRegisteredSeparately()
        {
            var container = new Catalog()
                .Register<ITwoTypeArgGenericInterface<string, T1>>(c => new TwoTypeArgGenericImpl<string, T1>())
                .Register<ITwoTypeArgGenericInterface<object, T1>>(c => new OtherTwoTypeArgGenericImpl<object, T1>())
                .CreateContainer();

            container.Resolve<ITwoTypeArgGenericInterface<object, string>>()
                .Should().BeOfType<OtherTwoTypeArgGenericImpl<object, string>>();

            container.Resolve<ITwoTypeArgGenericInterface<string, string>>()
                .Should().BeOfType<TwoTypeArgGenericImpl<string, string>>();
        }

        [Fact]
        public void SeparateClosedGenericsAreSeparateSingletons()
        {
            using (var singleton = new LifetimeContainer())
            {
                var container = new Catalog()
                    .Register<IOneTypeArgGenericInterface<T0>>(c => new OneTypeArgGenericImpl<T0>(), singleton)
                    .CreateContainer();

                var cg1 = container.Resolve<IOneTypeArgGenericInterface<object>>();
                var cgA = container.Resolve<IOneTypeArgGenericInterface<string>>();
                var cg2 = container.Resolve<IOneTypeArgGenericInterface<object>>();
                var cgB = container.Resolve<IOneTypeArgGenericInterface<string>>();

                cg1.Should().BeSameAs(cg2);
                cgA.Should().BeSameAs(cgB);
            }
        }

        [Fact]
        public void ResolvingOpenGenericsWithDependenciesResolvesCorrectly()
        {
            var container = new Catalog()
                .Register<IOneTypeArgGenericInterface<T0>>(c => new OtherOneArgGenericImpl<T0>(c.Resolve<T0>()))
                .Register(c => "hello")
                .CreateContainer();

            var o = container.Resolve<IOneTypeArgGenericInterface<string>>();
            o.Should().BeOfType<OtherOneArgGenericImpl<string>>();
            (o as OtherOneArgGenericImpl<string>).Should().NotBeNull();
            ((OtherOneArgGenericImpl<string>) o).Dependency.Should().Be("hello");
        }

        class OtherTwoTypeArgGenericImpl<TFirst, TSecond> : ITwoTypeArgGenericInterface<TFirst, TSecond>
        {
            
        }

        class OtherOneArgGenericImpl<TFirst> : IOneTypeArgGenericInterface<TFirst>
        {
            public TFirst Dependency;

            public OtherOneArgGenericImpl(TFirst dependency)
            {
                Dependency = dependency;
            }
        }
    }
}
