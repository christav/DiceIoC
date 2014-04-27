using System;
using System.Linq.Expressions;

namespace DiceIoC
{
    /// <summary>
    /// Class implementing the public API for per-resolve lifetime.
    /// </summary>
    public static class PerResolve
    {
        private static readonly object keyLock = new object();
        private static int nextKey = 1;

        private class PerResolveLifetimeManager
        {
            private readonly int key;

            public PerResolveLifetimeManager()
            {
                lock (keyLock)
                {
                    key = nextKey++;
                }
            }

// ReSharper disable once UnusedMember.Local
            // Member is used via dynamically generated expression
            public object GetValue(IContainer container)
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
            public object SetValue(object value, IContainer container)
            {
                container.PerResolveObjects[key] = value;
                return value;
            }
        }

        public static Func<Expression<Func<IContainer, object>>, Expression<Func<IContainer, object>>> Lifetime
        {
            get { return factory => Lifetimes.Lifetime.RewriteForLifetime(factory, new PerResolveLifetimeManager()); }
        }
    }
}
