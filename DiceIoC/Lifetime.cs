using System;
using System.Linq.Expressions;

namespace DiceIoC
{
    /// <summary>
    /// Base class for lifetimes that handles the details of
    /// the expression tree munging so lifetime implementors
    /// can concentrate on the details of lifetime storage.
    /// </summary>
    public abstract class Lifetime : ILifetime
    {
        public abstract object GetValue(Container c);
        public abstract object SetValue(object value, Container ce);

        protected Expression<Func<Container, object>> LifetimeModifier(
            Expression<Func<Container, object>> factoryExpression)
        {
            var c = Expression.Parameter(typeof(Container), "container");
            
            var ltm = Expression.Constant(this, typeof (ILifetime));
            var body = Expression.Coalesce(
                Expression.Call(ltm, "GetValue", null, c),
                Expression.Call(ltm, "SetValue", null,
                    Expression.Invoke(factoryExpression, c),
                    c));

            var final = Expression.Lambda<Func<Container, object>>(
                body, c);
            return final;
        }
    }
}
