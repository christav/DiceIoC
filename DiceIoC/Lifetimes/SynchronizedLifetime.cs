using System;

namespace DiceIoC.Lifetimes
{
    /// <summary>
    /// A base class for lifetimes that
    /// acquire a lock when accessed.
    /// </summary>
    public abstract class SynchronizedLifetime
    {
        private readonly object padLock = new object();

        public IDisposable Enter(Container container)
        {
            return new LockUnlockDispose(padLock);
        }
    }
}
