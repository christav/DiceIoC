using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DiceIoC
{
    public sealed class LifetimeScope : IDisposable
    {
        private static int nextKey = 1;
        private readonly Dictionary<int, object> objects = new Dictionary<int, object>();

        public void Dispose()
        {
            
        }

        private static void EnsureScope(IContainer container)
        {
            if (container.CurrentScope == null)
            {
                throw new InvalidOperationException("Cannot resolve scoped lifetime without a scope");
            }
        }

        internal class ScopedLifetimeManager
        {
            private int key;

            public ScopedLifetimeManager()
            {
                key = Interlocked.Increment(ref LifetimeScope.nextKey);
            }

            public object GetValue(IContainer container)
            {
                EnsureScope(container);
                object value;

                if (container.CurrentScope.objects.TryGetValue(key, out value))
                {
                    return value;
                }
                return null;
            }

            public object SetValue(object value, IContainer container)
            {
                EnsureScope(container);

                container.CurrentScope.objects[key] = value;
                return value;
            }
        }
    }
}
