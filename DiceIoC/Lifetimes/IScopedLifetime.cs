using System;

namespace DiceIoC.Lifetimes
{
    public interface IScopedLifetime
    {
        IDisposable Enter();
        object GetValue(int key);
        object SetValue(int key, object value);
    }
}
