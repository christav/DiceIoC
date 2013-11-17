using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DiceIoC
{
    public class Passthrough
    {
        private readonly Action<Container, string, Type> action;

        private Passthrough(Action<Container, string, Type> action)
        {
            this.action = action;
        }

        private Expression<Func<Container, string, Type, object>> ActionModifier(Expression<Func<Container, string, Type, object>> factoryExpression)
        {
            var c = Expression.Parameter(typeof (Container), "container");
            var name = Expression.Parameter(typeof (string), "name");
            var type = Expression.Parameter(typeof (Type), "type");

            var actionExpression = Expression.Constant(this.action, typeof (Action<Container, string, Type>));
            var body = Expression.Block(typeof (object),
                Expression.Call(actionExpression, "Invoke", null, c, name, type),
                Expression.Invoke(factoryExpression, c, name, type));
            return Expression.Lambda<Func<Container, string, Type, object>>(body, c, name, type);
        }

        public static Func<Expression<Func<Container, string, Type, object>>, Expression<Func<Container, string, Type, object>>> Modifier(Action<Container, string, Type> action)
        {
            return new Passthrough(action).ActionModifier;
        }
    }
}
