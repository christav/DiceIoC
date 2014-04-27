using System;
using System.Linq.Expressions;

namespace DiceIoC.Lifetimes
{
    /// <summary>
    /// Helper class to turn a "Lifetime manager" object into a
    /// modifier for use in registration. A lifetime manager
    /// is any object that implements the following methods:
    /// 
    /// IDisposable Enter();
    /// object GetValue(Container c);
    /// object SetValue(object value, Container c);
    /// 
    /// No need for an interface or base class, just implement
    /// the methods as named above publicly and it's set.
    /// </summary>
    public static class Lifetime
    {
        /// <summary>
        /// Helper function for building lifetimes. This method will
        /// rewrite the given <paramref name="factoryExpression"/>
        /// so that it is wrapped in calls to the <paramref name="lifetimeContainer"/>
        /// methods correctly.
        /// </summary>
        /// <remarks>The lifetime object doesn't have to derive from Lifetime,
        /// or even implement ILifetime. It just needs to implement the
        /// three methods Enter, GetValue, SetValue.</remarks>
        /// <typeparam name="T">Type of the <paramref name="lifetimeContainer"/> to use.</typeparam>
        /// <param name="factoryExpression">Original factory expression</param>
        /// <param name="lifetimeContainer">Lifetime container object that will be called by
        /// the rewritten expression.</param>
        /// <returns>The new expression.</returns>
        public static Expression<Func<IContainer, object>> RewriteForLifetime<T>(
            Expression<Func<IContainer, object>> factoryExpression, 
            T lifetimeContainer)
        {
            var c = Expression.Parameter(typeof(IContainer), "container");
            var guard = Expression.Parameter(typeof(IDisposable), "guard");
            var ltm = Expression.Constant(lifetimeContainer, typeof(T));

            var body = Expression.Block(new[] { guard },
                CreateGuard(guard, ltm),
                Expression.TryFinally(
                    GetSetValueExpression(factoryExpression, c, ltm),
                    Expression.Call(guard, "Dispose", null)
                    )
                );

            var final = Expression.Lambda<Func<IContainer, object>>(
                body, c);
            return final;
        }

        private static Expression CreateGuard(ParameterExpression guardVariable, ConstantExpression ltm)
        {
            return Expression.Assign(guardVariable,
                Expression.Call(ltm, "Enter", null));
        }

        private static Expression GetSetValueExpression(Expression factory, ParameterExpression container, ConstantExpression ltm)
        {
            return Expression.Coalesce(
                Expression.Call(ltm, "GetValue", null, container),
                Expression.Call(ltm, "SetValue", null,
                    Expression.Invoke(factory, container),
                    container));
        }
    }
}
