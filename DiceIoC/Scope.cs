using System;
using System.Linq.Expressions;
using DiceIoC.Lifetimes;

namespace DiceIoC
{
    public static class Scope
    {
        public static Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>> Lifetime
        {
            get
            {
                return factory => Lifetimes.Lifetime.RewriteForLifetime(factory, new ScopedLifetimeManager());
            }
        }
    }
}
