using System;
using System.Linq.Expressions;
using System.Threading;

namespace DiceIoC
{
    /// <summary>
    /// Class implementing the public API for per-resolve lifetime.
    /// </summary>
    public static class PerResolve
    {
        private static int nextKey = 1;

        private class PerResolveLifetimeManager
        {
            private readonly int key;

            public PerResolveLifetimeManager()
            {
                key = Interlocked.Increment(ref nextKey);
            }

// ReSharper disable once UnusedMember.Local
            // Member is used via dynamically generated expression
            public object GetValue(Container container)
            {
                object result;
                if (container.PerResolveObjects.TryGetValue(key, out result))
                {
                    return result;
                }
                return null;
            }

// ReSharper disable once UnusedMember.Local
            // Member is used via dynamically generated expression
            public object SetValue(object value, Container container)
            {
                container.PerResolveObjects[key] = value;
                return value;
            }
        }

        public static Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>> Lifetime
        {
            get { return factory => Lifetimes.Lifetime.RewriteForLifetime(factory, new PerResolveLifetimeManager()); }
        }
    }
}
