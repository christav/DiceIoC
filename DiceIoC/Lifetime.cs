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
        public abstract object GetValue(Container c, string name, Type requestedType);
        public abstract object SetValue(object value, Container c, string name, Type requestedType);

        protected Expression<Func<Container, string, Type, object>> LifetimeModifier(
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
    }
}
