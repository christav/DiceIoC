using System;
using System.Linq.Expressions;
using DiceIoC.Catalogs;
using DiceIoC.Tests.SampleTypes;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Generics
{
    public class TypeRewritingVisitorTests
    {
        [Fact]
        public void RewriterDoesNotChangeNonGenericExpression()
        {
            var visitor = new GenericTypeRewritingVisitor(typeof (object));
            Expression<Func<Container, object>> sourceExpression =
                c => new ConcreteClass();

            var rewrittenExpression = visitor.Visit(sourceExpression);

            rewrittenExpression.Should().BeSameAs(sourceExpression);
        }

        [Fact]
        public void SimpleNewExpressionGetsRewritten()
        {
            var visitor = new GenericTypeRewritingVisitor(typeof (object));
            Expression<Func<Container, object>> sourceExpression =
                c => new GenericOneArgImpl<T0>();

            var rewrittenExpression = visitor.Visit(sourceExpression);

            rewrittenExpression.NodeType.Should().Be(ExpressionType.Lambda);

            var func = (Func<Container, object>)(((LambdaExpression) rewrittenExpression).Compile());
            object result = func(null);

            result.Should().BeOfType<GenericOneArgImpl<object>>();
        }


        private class GenericOneArgImpl<T> : IOneTypeArgGenericInterface<T>
        {
        }
    }
}
