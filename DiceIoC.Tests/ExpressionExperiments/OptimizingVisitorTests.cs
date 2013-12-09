using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.ExpressionExperiments
{
    public class OptimizingVisitorTests
    {
        [Fact]
        public void OptimizingExpressionsWithoutResolveReturnsOriginalExpression()
        {
            var factories = new Dictionary<RegistrationKey, Expression<Func<Container, string, Type, object>>>();
            Expression<Func<Container, string, Type, ConcreteClass>> e = (c, n, t) => new ConcreteClass();

            var visitor = new OptimizingVisitor(factories);
            var e2 = visitor.Visit(e);

            Assert.Same(e, e2);
        }

        [Fact]
        public void OptimizingExpressionReplacesDefaultResolves()
        {
            var factories = new Dictionary<RegistrationKey, Expression<Func<Container, string, Type, object>>>();
            factories[new RegistrationKey(null, typeof (ISimpleInterface))] = (c, n, t) => new SimpleInterfaceImpl();

            Expression<Func<Container, string, Type, ConcreteClassWithDependencies>> e = 
                (c, n, t) => new ConcreteClassWithDependencies(c.Resolve<ISimpleInterface>());

            var walker = new WalkingVisitor();
            walker.Visit(e);

            Assert.True(walker.found);
            walker.found = false;

            var visitor = new OptimizingVisitor(factories);
            var e2 = visitor.Visit(e);

            walker.Visit(e2);
            Assert.False(walker.found);
        }

        [Fact]
        public void OptimizedExpressionCanBeCompiled()
        {
            var factories = new Dictionary<RegistrationKey, Expression<Func<Container, string, Type, object>>>();
            factories[new RegistrationKey(null, typeof(ISimpleInterface))] = (c, n, t) => new SimpleInterfaceImpl();

            Expression<Func<Container, string, Type, object>> e =
                (c, n, t) => new ConcreteClassWithDependencies(c.Resolve<ISimpleInterface>());

            var visitor = new OptimizingVisitor(factories);
            var e2 = visitor.Visit(e);

            var factory = ((Expression<Func<Container, string, Type, object>>) e2).Compile();
            object result = factory(null, null, typeof (ConcreteClassWithDependencies));

            Assert.NotNull(result);
            Assert.IsType<ConcreteClassWithDependencies>(result);
        }
    }
}
