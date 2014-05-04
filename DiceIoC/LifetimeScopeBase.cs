using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceIoC
{
    public abstract class LifetimeScopeBase : IScopedLifetime, IDisposable
    {
        private Dictionary<int, object> objects = new Dictionary<int, object>();

        public void Dispose()
        {
            foreach (IDisposable d in objects.Values.OfType<IDisposable>())
            {
                d.Dispose();
            }
            objects = new Dictionary<int, object>();
        }

        public abstract IDisposable Enter();

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
}