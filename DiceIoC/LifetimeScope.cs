using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DiceIoC.Lifetimes;

namespace DiceIoC
{
    public sealed class LifetimeScope : IScopedLifetime, IDisposable
    {
        private readonly Dictionary<int, object> objects = new Dictionary<int, object>();

        public void Dispose()
        {

        }

        public IDisposable Enter()
        {
            return new NoopDispose();
        }

        public object GetValue(int key)
        {
            object result;
            if (objects.TryGetValue(key, out result))
            {
                return result;
            }
            return null;
        }

        public object SetValue(int key, object value)
        {
            objects[key] = value;
            return value;
        }
    }

    public class ScopedLifetimeManager
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
