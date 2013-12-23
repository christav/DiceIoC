using System;
using System.Linq.Expressions;

namespace DiceIoC
{
    public class Passthrough
    {
        private readonly Action<Container> action;

        private Passthrough(Action<Container> action)
        {
            this.action = action;
        }

        private Expression<Func<Container, object>> ActionModifier(Expression<Func<Container, object>> factoryExpression)
        {
            var c = Expression.Parameter(typeof (Container), "container");

            var actionExpression = Expression.Constant(action, typeof (Action<Container>));
            var body = Expression.Block(typeof (object),
                Expression.Call(actionExpression, "Invoke", null, c),
                Expression.Invoke(factoryExpression, c));
            return Expression.Lambda<Func<Container, object>>(body, c);
        }

        public static Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>> Modifier(Action<Container> action)
        {
            return new Passthrough(action).ActionModifier;
        }
    }
}
