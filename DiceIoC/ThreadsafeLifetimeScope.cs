using System;
using DiceIoC.Lifetimes;

namespace DiceIoC
{
    public class ThreadsafeLifetimeScope : LifetimeScopeBase
    {
        private readonly object padLock = new object();

        public override IDisposable Enter()
        {
            return new LockUnlockDispose(padLock);
        }
    }
}
