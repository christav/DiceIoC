using System;
using FluentAssertions;
using Xunit;
using System.Linq.Expressions;
using DiceIoC.Tests.SampleTypes;

namespace DiceIoC.Tests.ExpressionExperiments
{


    public class Expressions : IDisposable
    {
        public Expressions()
        {
        }

        public void Dispose()
        {
        }

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

            var container = new Container();
            var resultObj = result(container, "foo", typeof (ConcreteClass));

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
    }
}
