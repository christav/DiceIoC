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

        private static void EnsureScope(IContainer container)
        {
            if (container.CurrentScope == null)
            {
                throw new InvalidOperationException("Cannot resolve scoped lifetime without a scope");
            }
        }

        public IDisposable Enter(IContainer container)
        {
            EnsureScope(container);
            return container.CurrentScope.Enter();
        }

        public object GetValue(IContainer container)
        {
            EnsureScope(container);
            return container.CurrentScope.GetValue(key);
        }

        public object SetValue(object value, IContainer container)
        {
            EnsureScope(container);
            return container.CurrentScope.SetValue(key, value);
        }
    }
}