using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;
using System.Linq.Expressions;
using DiceIoC.Tests.SampleTypes;

namespace DiceIoC.Tests.ExpressionExperiments
{


    public class Expressions
    {
        [Fact]
        public void SpelunkIntoLambda()
        {
            Expression<Func<Container, ConcreteClass>> factory = (c0) => new ConcreteClass();
            var c = Expression.Parameter(typeof (Container), "container");
            var name = Expression.Parameter(typeof (string), "name");
            var t = Expression.Parameter(typeof (Type), "resolvedType");

            LambdaExpression wrapped =
                Expression.Lambda<Func<Container, string, Type, ConcreteClass>>(
                    Expression.Invoke(factory, c),
                    c, name, t
                    );
            var result = (Func<Container, String, Type, ConcreteClass>)wrapped.Compile();

            var resultObj = result(null, "foo", typeof (ConcreteClass));

            Assert.NotNull(resultObj);
            Assert.IsType<ConcreteClass>(resultObj);

            Assert.NotNull(factory);
        }

        [Fact]
        public void WhatDoTypeCastsLookLikeInExpressions()
        {
            Expression<Func<object>> factory = () => (object)new ConcreteClass();

            factory.Body.NodeType.Should().Be(ExpressionType.Convert);

        }

        [Fact]
        public void CanVisitAndFindResolveCall()
        {
            Expression<Func<Container, string, Type, object>> expr =
                (container, name, t) => new ConcreteClassWithDependencies(container.Resolve<ISimpleInterface>("withname"));

            var visitor = new WalkingVisitor();
            visitor.Visit(expr);
            Assert.True(visitor.found);
        }
    }

    class WalkingVisitor : ExpressionVisitor
    {
        private static List<MethodInfo> resolveMethods = typeof (Container).GetMethods().Where(m => m.Name == "Resolve").ToList();
        public bool found = false;

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            return base.VisitNew(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsGenericMethod && resolveMethods.Contains(node.Method.GetGenericMethodDefinition()))
            {
                found = true;
            }
            return base.VisitMethodCall(node);
        }
    }
}
