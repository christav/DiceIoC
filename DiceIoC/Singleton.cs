using System;
using System.Linq.Expressions;

namespace DiceIoC
{
    public class Singleton : Lifetime, IDisposable
    {
        private object value;

        public static Func<Expression<Func<Container, string, Type, object>>, 
            Expression<Func<Container, string, Type, object>>> Lifetime()
        {
            return new Singleton().LifetimeModifier;
        }

        public void Dispose()
        {
            if (value is IDisposable)
            {
                ((IDisposable) value).Dispose();
                value = null;
            }
        }

        public override object GetValue(Container c, string name, Type requestedType)
        {
            return value;
        }

        public override object SetValue(object value, Container c, string name, Type requestedType)
        {
            this.value = value;
            return value;
        }
    }
}
