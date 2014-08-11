using System;
using System.Linq.Expressions;

namespace DiceIoC.Lifetimes
{
    /// <summary>
    /// Helper class to turn a "Lifetime manager" object into a
    /// modifier for use in registration. A lifetime manager
    /// is any object that implements the following methods:
    /// 
    /// IDisposable Enter(Container c);
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
        /// so that it is wrapped in calls to the <paramref name="lifetimeManager"/>
        /// methods correctly.
        /// </summary>
        /// <remarks>The lifetime object doesn't have to derive from Lifetime,
        /// or even implement ILifetime. It just needs to implement the
        /// three methods Enter, GetValue, SetValue.</remarks>
        /// <typeparam name="T">Type of the <paramref name="lifetimeManager"/> to use.</typeparam>
        /// <param name="factoryExpression">Original factory expression</param>
        /// <param name="lifetimeManager">Lifetime container object that will be called by
        /// the rewritten expression.</param>
        /// <returns>The new expression.</returns>
        public static Expression<Func<Container, object>> RewriteForLifetime<T>(
            Expression<Func<Container, object>> factoryExpression, 
            T lifetimeManager)
        {
            Expression body;
            var c = Expression.Parameter(typeof(Container), "container");

            if (typeof (T).GetMethod("Enter") != null)
            {
                body = BodyUsingEnter(c, factoryExpression, lifetimeManager);
            }
            else
            {
                body = BodyWithoutEnter(c, factoryExpression, lifetimeManager);
            }

            var final = Expression.Lambda<Func<Container, object>>(
                body, c);
            return final;
        }

        private static Expression BodyUsingEnter<T>(
            ParameterExpression container,
            Expression<Func<Container, object>> factoryExpression,
            T lifetimeManager)
        {
            var guard = Expression.Parameter(typeof(IDisposable), "guard");
            var ltm = Expression.Constant(lifetimeManager, typeof(T));

            var body = Expression.Block(new[] { guard },
                CreateGuard(guard, container, ltm),
                Expression.TryFinally(
                    GetSetValueExpression(factoryExpression, container, ltm),
                    Expression.Call(guard, "Dispose", null)
                    )
                );

            return body;
        }

        private static Expression BodyWithoutEnter<T>(ParameterExpression container,
            Expression<Func<Container, object>> factoryExpression,
            T lifetimeManager)
        {
            var ltm = Expression.Constant(lifetimeManager, typeof (T));
            return GetSetValueExpression(factoryExpression, container, ltm);
        }

        private static Expression CreateGuard(ParameterExpression guardVariable, ParameterExpression container, ConstantExpression ltm)
        {
            return Expression.Assign(guardVariable,
                Expression.Call(ltm, "Enter", null, container));
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
