using System;
using System.Linq.Expressions;

namespace DiceIoC
{
    /// <summary>
    /// Base class for lifetimes that handles the details of
    /// the expression tree munging so lifetime implementors
    /// can concentrate on the details of lifetime storage.
    /// 
    /// Locking is explicitly not implemented (the Enter method
    /// is a noop).
    /// </summary>
    public abstract class Lifetime : ILifetime
    {
        private class NoopDispose : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public virtual IDisposable Enter()
        {
            return new NoopDispose();
        }

        public abstract object GetValue(Container c);
        public abstract object SetValue(object value, Container ce);

        public Expression<Func<Container, object>> LifetimeModifier(
            Expression<Func<Container, object>> factoryExpression)
        {
            var c = Expression.Parameter(typeof(Container), "container");
            var guard = Expression.Parameter(typeof (IDisposable), "guard");
            var ltm = Expression.Constant(this, typeof (ILifetime));

            var body = Expression.Block(new [] {guard},
                CreateGuard(guard, ltm),
                Expression.TryFinally(
                    GetSetValueExpression(factoryExpression, c, ltm),
                    Expression.Call(guard, "Dispose", null)
                    )
                );

            var final = Expression.Lambda<Func<Container, object>>(
                body, c);
            return final;
        }

        public Expression CreateGuard(ParameterExpression guardVariable, ConstantExpression ltm)
        {
            return Expression.Assign(guardVariable,
                Expression.Call(ltm, "Enter", null));
        }

        public Expression GetSetValueExpression(Expression factory, ParameterExpression container, ConstantExpression ltm)
        {
            return Expression.Coalesce(
                Expression.Call(ltm, "GetValue", null, container),
                Expression.Call(ltm, "SetValue", null,
                    Expression.Invoke(factory, container),
                    container));
        }
    }
}
