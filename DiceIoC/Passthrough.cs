using System;
using System.Linq.Expressions;

namespace DiceIoC
{
    public class Passthrough
    {
        private readonly Action<IContainer> action;

        private Passthrough(Action<IContainer> action)
        {
            this.action = action;
        }

        private Expression<Func<IContainer, object>> ActionModifier(Expression<Func<IContainer, object>> factoryExpression)
        {
            var c = Expression.Parameter(typeof (IContainer), "IContainer");

            var actionExpression = Expression.Constant(action, typeof (Action<IContainer>));
            var body = Expression.Block(typeof (object),
                Expression.Call(actionExpression, "Invoke", null, c),
                Expression.Invoke(factoryExpression, c));
            return Expression.Lambda<Func<IContainer, object>>(body, c);
        }

        public static Func<Expression<Func<IContainer, object>>, Expression<Func<IContainer, object>>> Modifier(Action<IContainer> action)
        {
            return new Passthrough(action).ActionModifier;
        }
    }
}
