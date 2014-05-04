using System;
using DiceIoC.Lifetimes;

namespace DiceIoC
{
    public sealed class LifetimeScope : LifetimeScopeBase
    {
        public override IDisposable Enter()
        {
            return new NoopDispose();
        }
    }
}
