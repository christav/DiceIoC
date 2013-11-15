using System;

namespace DiceIoC
{
    public class Singleton
    {
        private object value;
        private Func<Container, string, Type, object> factory;
 
        private Func<Container, string, Type, object> LifetimeModifier(Func<Container, string, Type, object> factory)
        {
            this.factory = factory;
            return this.GetValue;
        }

        private object GetValue(Container c, string name, Type t)
        {
            return value ?? (value = factory(c, name, t));
        }

        public static Func<Func<Container, string, Type, object>, Func<Container, string, Type, object>> Lifetime()
        {
            return new Singleton().LifetimeModifier;
        }
    }
}
