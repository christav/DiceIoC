using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using DiceIoC.Catalogs;
using DiceIoC.Tests.ExpressionExperiments;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Basics
{
    public class ResolveCallInliningVisitorTests
    {
        private class DictionaryCatalog : ICatalog, 
            IEnumerable<KeyValuePair<RegistrationKey, Expression<Func<IContainer, object>>>>
        {
            private readonly Dictionary<RegistrationKey, Expression<Func<IContainer, object>>> factories =
                new Dictionary<RegistrationKey, Expression<Func<IContainer, object>>>();

            public void Add(RegistrationKey key, Expression<Func<IContainer, object>> factory)
            {
                factories[key] = factory;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<KeyValuePair<RegistrationKey, Expression<Func<IContainer, object>>>> GetEnumerator()
            {
                return factories.GetEnumerator();
            }

            public IEnumerable<KeyValuePair<RegistrationKey, Expression<Func<IContainer, object>>>> GetFactoryExpressions()
            {
                throw new NotImplementedException();
            }

            public Expression<Func<IContainer, object>> GetFactoryExpression(RegistrationKey key)
            {
                Expression<Func<IContainer, object>> factory;
                factories.TryGetValue(key, out factory);
                return factory;
            }

            public IEnumerable<Expression<Func<IContainer, object>>> GetFactoryExpressions(Type serviceType)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void OptimizingExpressionsWithoutResolveReturnsOriginalExpression()
        {
            var factories = new DictionaryCatalog();
            Expression<Func<IContainer, ConcreteClass>> e = c => new ConcreteClass();

            var visitor = new ResolveCallInliningVisitor(factories);
            var e2 = visitor.Visit(e);

            Assert.Same(e, e2);
        }

        [Fact]
        public void OptimizingExpressionReplacesDefaultResolves()
        {
            var factories = new DictionaryCatalog
            {
                {RegistrationKey.For<ISimpleInterface>(null), c => new SimpleInterfaceImpl()}
            };

            Expression<Func<IContainer, ConcreteClassWithDependencies>> e = 
                c => new ConcreteClassWithDependencies(c.Resolve<ISimpleInterface>());

            var walker = new WalkingVisitor();
            walker.Visit(e);

            walker.Found.Should().BeTrue();
            walker.Found = false;

            var visitor = new ResolveCallInliningVisitor(factories);
            var e2 = visitor.Visit(e);

            walker.Visit(e2);
            walker.Found.Should().BeFalse();
        }


        [Fact]
        public void ExpressionsResolvingWithNameGetReplaced()
        {
            var factories = new DictionaryCatalog
            {
                {RegistrationKey.For<ISimpleInterface>("named"), c => new SimpleInterfaceImpl()}
            };

            Expression<Func<IContainer, ConcreteClassWithDependencies>> e =
                c => new ConcreteClassWithDependencies(c.Resolve<ISimpleInterface>("named"));

            var walker = new WalkingVisitor();
            walker.Visit(e);

            walker.Found.Should().BeTrue();

            walker.Found = false;

            var visitor = new ResolveCallInliningVisitor(factories);
            var e2 = visitor.Visit(e);

            walker.Visit(e2);
            walker.Found.Should().BeFalse();
        }

        [Fact]
        public void OptimizedExpressionCanBeCompiled()
        {
            var factories = new DictionaryCatalog
            {
                {RegistrationKey.For<ISimpleInterface>(), c => new SimpleInterfaceImpl()}
            };

            Expression<Func<IContainer, object>> e =
                c => new ConcreteClassWithDependencies(c.Resolve<ISimpleInterface>());

            var visitor = new ResolveCallInliningVisitor(factories);
            var e2 = visitor.Visit(e);

            var factory = ((Expression<Func<IContainer,object>>) e2).Compile();
            object result = factory(null);

            result.Should().NotBeNull();
            result.Should().BeOfType<ConcreteClassWithDependencies>();
        }

    }
}
