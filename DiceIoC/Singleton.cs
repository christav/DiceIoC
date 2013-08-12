using System;

namespace DiceIoC
{
    public class Singleton
    {
        private object value;

        private Func<Container, string, Type, object> LifetimeModifier(Func<Container, string, Type, object> factory)
        {
            return (c, n, t) => value ?? (value = factory(c, n, t));
        }

        public static Func<Func<Container, string, Type, object>, Func<Container, string, Type, object>> Lifetime()
        {
            var l = new Singleton();
            return l.LifetimeModifier;
        }
    }
}
