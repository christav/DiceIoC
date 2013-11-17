using System;
using System.Linq.Expressions;

namespace DiceIoC
{
    public class Singleton : ILifetime
    {
        private object value;
 
        private Expression<Func<Container, string, Type, object>> LifetimeModifier(
            Expression<Func<Container, string, Type, object>> factoryExpression)
        {
            var c = Expression.Parameter(typeof(Container), "container");
            var name = Expression.Parameter(typeof (string), "name");
            var type = Expression.Parameter(typeof (Type), "type");
            
            var ltm = Expression.Constant(this, typeof (ILifetime));
            var body = Expression.Coalesce(
                Expression.Call(ltm, "GetValue", null, c, name, type),
                Expression.Call(ltm, "SetValue", null,
                    Expression.Invoke(factoryExpression, c, name, type),
                    c, name, type));

            var final = Expression.Lambda<Func<Container, string, Type, object>>(
                body, c, name, type);
            return final;
        }

        public static Func<Expression<Func<Container, string, Type, object>>, 
            Expression<Func<Container, string, Type, object>>> Lifetime()
        {
            return new Singleton().LifetimeModifier;
        }

        public void Dispose()
        {
            if (value is IDisposable)
            {
                ((IDisposable) value).Dispose();
                value = null;
            }
        }

        object ILifetime.GetValue(Container c, string name, Type requestedType)
        {
            return value;
        }

        public object SetValue(object value, Container c, string name, Type requestedType)
        {
            this.value = value;
            return value;
        }
    }
}
