using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DiceIoC.Tests.ExpressionExperiments;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Basics
{
    public class ResolveCallInliningVisitorTests
    {
        [Fact]
        public void OptimizingExpressionsWithoutResolveReturnsOriginalExpression()
        {
            var factories = new Dictionary<RegistrationKey, Expression<Func<Container, object>>>();
            Expression<Func<Container, ConcreteClass>> e = c => new ConcreteClass();

            var visitor = new ResolveCallInliningVisitor(factories);
            var e2 = visitor.Visit(e);

            Assert.Same(e, e2);
        }

        [Fact]
        public void OptimizingExpressionReplacesDefaultResolves()
        {
            var factories = new Dictionary<RegistrationKey, Expression<Func<Container, object>>>();
            factories[new RegistrationKey(typeof (ISimpleInterface), null)] = c => new SimpleInterfaceImpl();

            Expression<Func<Container, ConcreteClassWithDependencies>> e = 
                c => new ConcreteClassWithDependencies(c.Resolve<ISimpleInterface>());

            var walker = new WalkingVisitor();
            walker.Visit(e);

            walker.found.Should().BeTrue();
            walker.found = false;

            var visitor = new ResolveCallInliningVisitor(factories);
            var e2 = visitor.Visit(e);

            walker.Visit(e2);
            walker.found.Should().BeFalse();
        }


        [Fact]
        public void ExpressionsResolvingWithNameGetReplaced()
        {
            var factories = new Dictionary<RegistrationKey, Expression<Func<Container, object>>>();
            factories[new RegistrationKey(typeof(ISimpleInterface), "named")] = c => new SimpleInterfaceImpl();

            Expression<Func<Container, ConcreteClassWithDependencies>> e =
                c => new ConcreteClassWithDependencies(c.Resolve<ISimpleInterface>("named"));

            var walker = new WalkingVisitor();
            walker.Visit(e);

            walker.found.Should().BeTrue();

            walker.found = false;

            var visitor = new ResolveCallInliningVisitor(factories);
            var e2 = visitor.Visit(e);

            walker.Visit(e2);
            walker.found.Should().BeFalse();
        }

        [Fact]
        public void OptimizedExpressionCanBeCompiled()
        {
            var factories = new Dictionary<RegistrationKey, Expression<Func<Container, object>>>();
            factories[new RegistrationKey(typeof(ISimpleInterface), null)] = c => new SimpleInterfaceImpl();

            Expression<Func<Container, object>> e =
                c => new ConcreteClassWithDependencies(c.Resolve<ISimpleInterface>());

            var visitor = new ResolveCallInliningVisitor(factories);
            var e2 = visitor.Visit(e);

            var factory = ((Expression<Func<Container,object>>) e2).Compile();
            object result = factory(null);

            result.Should().NotBeNull();
            result.Should().BeOfType<ConcreteClassWithDependencies>();
        }

    }
}
