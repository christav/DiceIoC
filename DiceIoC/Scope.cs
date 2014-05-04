using System;
using System.Linq.Expressions;

namespace DiceIoC
{
    public static class Scope
    {
        public static Func<Expression<Func<IContainer, object>>, Expression<Func<IContainer, object>>> Lifetime
        {
            get
            {
                return factory => Lifetimes.Lifetime.RewriteForLifetime(factory, new ScopedLifetimeManager());
            }
        }
    }
}
