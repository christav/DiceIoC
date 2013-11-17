using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiceIoC
{
    public interface ILifetime
    {
        object GetValue(Container c, string name, Type requestedType);
        object SetValue(object value, Container c, string name, Type requestedType);
    }
}
