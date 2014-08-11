using System;
using System.Threading;

namespace DiceIoC.Lifetimes
{
    internal class ScopedLifetimeManager
    {
        private static int nextKey = 1;
        private readonly int key;

        public ScopedLifetimeManager()
        {
            this.key = Interlocked.Increment(ref nextKey);
        }

        private static void EnsureScope(Container container)
        {
            if (container.CurrentScope == null)
            {
                throw new InvalidOperationException("Cannot resolve scoped lifetime without a scope");
            }
        }

        public IDisposable Enter(Container container)
        {
            EnsureScope(container);
            return container.CurrentScope.Enter();
        }

        public object GetValue(Container container)
        {
            EnsureScope(container);
            return container.CurrentScope.GetValue(key);
        }

        public object SetValue(object value, Container container)
        {
            EnsureScope(container);
            return container.CurrentScope.SetValue(key, value);
        }
    }
}