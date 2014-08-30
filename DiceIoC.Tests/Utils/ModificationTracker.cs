using System;
using System.Linq.Expressions;

namespace DiceIoC.Tests.Utils
{
    public class ModificationTracker
    {
        public int ModifierCallCount = 0;
        public Expression<Func<Container, object>> LastFactoryExpression;

        public Expression<Func<Container, object>> Modifier(Expression<Func<Container, object>> factory)
        {
            ++ModifierCallCount;
            LastFactoryExpression = factory;
            return factory;
        }
    }
}