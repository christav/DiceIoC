using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DiceIoC
{
    /// <summary>
    /// A base class for lifetimes that
    /// acquire a lock when accessed.
    /// </summary>
    public abstract class SynchronizedLifetime : Lifetime
    {
        private readonly object padLock = new object();

        private class PadLock : IDisposable
        {
            private readonly object padLock;

            public PadLock(object padLock)
            {
                this.padLock = padLock;
                Monitor.Enter(padLock);
            }

            public void Dispose()
            {
                Monitor.Exit(this.padLock);
            }
        }

        public override IDisposable Enter()
        {
            return new PadLock(this.padLock);
        }
    }
}
