using System;
using System.Linq.Expressions;
using DiceIoC.Catalogs;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Generics
{
    public class RegistrationAndCatalogs
    {
        class Implementation<T> : IOneTypeArgGenericInterface<T>
        {
            
        }

        [Fact]
        public void BasicCatalogRejectsOpenGenericRegistrations()
        {
            var catalog = new BasicCatalog();
            catalog.Register<IOneTypeArgGenericInterface<T0>>(c => new Implementation<T0>());

            catalog.GetFactoryExpression(RegistrationKey.For<IOneTypeArgGenericInterface<T0>>()).Should().BeNull();
        }

        [Fact]
        public void OpenGenericCatalogAcceptsGenericRegistrations()
        {
            var catalog = new OpenGenericCatalog();
            catalog.Register<IOneTypeArgGenericInterface<T0>>(c => new Implementation<T0>());

            catalog.GetFactoryExpression(RegistrationKey.For<IOneTypeArgGenericInterface<object>>()).Should().NotBeNull();
        }

        [Fact]
        public void RetrievedFactoryIsConvertedToTargetType()
        {
            var catalog = new OpenGenericCatalog();
            catalog.Register<IOneTypeArgGenericInterface<T0>>(c => new Implementation<T0>());

            var factoryExpression = catalog.GetFactoryExpression(RegistrationKey.For<IOneTypeArgGenericInterface<object>>());
            
            var factory = (Func<Container, object>)((LambdaExpression)factoryExpression).Compile();
            factory(null).Should().BeOfType<Implementation<object>>();
        }
    }
}
