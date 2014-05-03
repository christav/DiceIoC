using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DiceIoC
{
    public static class Scope
    {
        public static Func<Expression<Func<IContainer, object>>, Expression<Func<IContainer, object>>> Lifetime
        {
            get { return e => e; }
        }
    }
}
