using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiceIoC
{
    public interface IScopedLifetime
    {
        IDisposable Enter();
        object GetValue(int key);
        object SetValue(int key, object value);
    }
}
